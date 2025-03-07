using Dapper;
using System.Data;
using TeamTactics.Application.Common.Exceptions;
using TeamTactics.Application.Teams;
using TeamTactics.Domain.Teams;

namespace TeamTactics.Infrastructure.Database.Repositories
{
    class TeamRepository(IDbConnection dbConnection) : ITeamRepository
    {
        private IDbConnection _dbConnection = dbConnection;

        public Task<IEnumerable<Team>> FindAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Team?> FindByIdAsync(int id)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            var parameters = new DynamicParameters();
            parameters.Add("Id", id);

            string sql = $@"
    SELECT 
        t.id, 
        t.name, 
        t.status, 
        t.user_account_id, 
        t.user_tournament_id,
        tp.player_id,
        tp.captain,
	    c.id as club_id
    FROM 
        team_tactics.user_team t
    LEFT JOIN 
        team_tactics.player_user_team tp ON t.id = tp.user_team_id
    INNER JOIN team_tactics.player p ON tp.player_id = p.id
    INNER JOIN team_tactics.player_contract pc ON p.id = pc.player_id and pc.active = true
    INNER JOIN team_tactics.club c ON pc.club_id = c.id
    WHERE 
        t.id = @Id";

            var results = await _dbConnection.QueryAsync<dynamic>(sql, parameters);

            if (!results.Any())
                return null;

            var teamPlayers = new List<TeamPlayer>();

            // Add players (if any)
            foreach (var row in results)
            {
                if (row.player_id != null)
                    teamPlayers.Add(new TeamPlayer(row.player_id, row.club_id, row.captain));

            }

            // Get the first row to populate team details
            var firstRow = results.First();

            // Parse team status
            Enum.TryParse(firstRow.status.ToString(), out TeamStatus teamStatus);

            Team team = new Team(id, firstRow.name, teamStatus, firstRow.user_account_id, firstRow.user_tournament_id, teamPlayers);

            return team;
        }

        //returner alle teams som brugeren ejer
        public async Task<IEnumerable<Team>> FindUserTeamsAsync(int userId)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);

            string sql = @"
    SELECT 
        t.id, 
        t.name, 
        t.status, 
        t.user_account_id, 
        t.user_tournament_id,
        p.player_id,
        p.captain
    FROM 
        team_tactics.user_team t
    LEFT JOIN 
        team_tactics.player_user_team p ON t.id = p.user_team_id
    WHERE 
        t.user_account_id = @UserId
    ORDER BY 
        t.id";

            var results = await _dbConnection.QueryAsync<dynamic>(sql, parameters);

            // Group results by team ID
            var teamGroups = results.GroupBy(r => (int)r.id);
            var teams = new List<Team>();

            foreach (var group in teamGroups)
            {
                var teamId = group.Key;
                var firstRow = group.First();

                Enum.TryParse(firstRow.status.ToString(), out TeamStatus teamStatus);

                var players = new List<TeamPlayer>();
                foreach (var row in group)
                {
                    if (row.player_id != null)
                    {
                        players.Add(new TeamPlayer(row.player_id, teamId, row.captain));
                    }
                }

                teams.Add(new Team(teamId, firstRow.name, teamStatus, firstRow.user_account_id, firstRow.user_tournament_id, players));
            }

            return teams;
        }

        public async Task<int> InsertAsync(Team team)
        {
            return await UpsertAsync(team);
        }

        public async Task RemoveAsync(int id)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            var parameters = new DynamicParameters();
            parameters.Add("Id", id);

            //ON DELETE CASCADE deletes all player_user_team associated with the team
            string sql = @"DELETE FROM team_tactics.user_team WHERE id = @Id";

            int rowsAffeted = await _dbConnection.ExecuteAsync(sql, parameters);
            if (rowsAffeted == 0)
            {
                throw EntityNotFoundException.ForEntity<Team>(id, nameof(Team.Id));
            }
        }

        //opdater name og status, når status = locked, opdater locked_date. 
        //Opdate player_user_team til at reflektere player listen
        public async Task UpdateAsync(Team team)
        {
            await UpsertAsync(team);
        }

        private async Task<int> UpsertAsync(Team model)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            using var transaction = _dbConnection.BeginTransaction();

            try
            {
                DateOnly? lockedDate = null;

                // TODO: This sets the locked whenever the team is updated, should only be set when the status is changed to locked.
                // This should be handled in the domian lock method.
                if (model.Status == TeamStatus.Locked)
                {
                    lockedDate = DateOnly.FromDateTime(DateTime.UtcNow);
                }

                var parameters = new DynamicParameters();
                string teamSql;
                if (model.Id == 0)
                {
                    teamSql = @"
    INSERT INTO team_tactics.user_team
        (name, status, locked_date, user_account_id, user_tournament_id)
    VALUES
        (@Name, @Status, @LockedDate, @UserId, @TournamentId)
    RETURNING id";
                }
                else
                {
                    parameters.Add("Id", model.Id);
                    teamSql = @"
    UPDATE team_tactics.user_team SET
        name = @Name,
        status = @Status,
        locked_date = @LockedDate,
        user_account_id = @UserId,
        user_tournament_id = @TournamentId
    WHERE
        id = @Id
    RETURNING id";
                }

                //parameters.Add("Id", model.Id > 0 ? model.Id : (object)DBNull.Value);
                parameters.Add("Name", model.Name);
                parameters.Add("Status", (int)model.Status);
                parameters.Add("LockedDate", lockedDate);
                parameters.Add("UserId", model.UserId);
                parameters.Add("TournamentId", model.TournamentId);

                int teamId = await _dbConnection.QuerySingleOrDefaultAsync<int>(teamSql, parameters, transaction);
                if (teamId == 0 && model.Id > 0)
                {
                    throw EntityNotFoundException.ForEntity<Team>(teamId, nameof(Team.Id));
                }
                else if (teamId == 0)
                {
                    throw new Exception("Failed to get returned team id on insert team.");
                }

                string deletePlayersSql = @"
            DELETE FROM team_tactics.player_user_team
            WHERE user_team_id = @TeamId";

                await _dbConnection.ExecuteAsync(deletePlayersSql, new { TeamId = teamId }, transaction);

                if (model.Players.Any())
                {
                    var playerValues = new List<string>();
                    var playerParameters = new DynamicParameters();

                    int i = 0;
                    foreach (var player in model.Players)
                    {
                        playerValues.Add($"(@PlayerId{i}, @TeamId, @Captain{i})");

                        playerParameters.Add($"PlayerId{i}", player.PlayerId);
                        playerParameters.Add($"Captain{i}", player.IsCaptain);
                        i++;
                    }

                    playerParameters.Add("TeamId", teamId);

                    string playerSql = $@"
                INSERT INTO team_tactics.player_user_team (player_id, user_team_id, captain)
                VALUES {string.Join(", ", playerValues)}";

                    await _dbConnection.ExecuteAsync(playerSql, playerParameters, transaction);
                }

                transaction.Commit();
                model.SetId(teamId);
                return teamId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
