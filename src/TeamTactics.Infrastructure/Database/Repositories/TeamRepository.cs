using Dapper;
using System.Data;
using TeamTactics.Application.Common.Exceptions;
using TeamTactics.Application.Points;
using TeamTactics.Application.Teams;
using TeamTactics.Domain.Teams;
using TeamTactics.Domain.Tournaments;

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
        t.formation,
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

            var results = await _dbConnection.QueryAsync<(int teamId, string teamName, string teamStatus, string teamFormation, int teamUserId, int teamTourneyId, int? teamPlayerId, bool? teamPlayerIsCaptain, int? clubId)>(sql, parameters);

            if (!results.Any())
                return null;

            var teamPlayers = new List<TeamPlayer>();

            // Add players (if any)
            foreach (var row in results)
            {
                if (row.teamPlayerId.HasValue && row.clubId.HasValue && row.teamPlayerIsCaptain.HasValue)
                    teamPlayers.Add(new TeamPlayer(row.teamPlayerId.Value, row.clubId.Value, row.teamPlayerIsCaptain.Value));
            }

            // Get the first row to populate team details
            var firstRow = results.First();

            // Parse team status
            Enum.TryParse(firstRow.teamStatus.ToString(), out TeamStatus teamStatus);

            Team team = new Team(id, firstRow.teamName, teamStatus, firstRow.teamFormation, firstRow.teamUserId, firstRow.teamTourneyId, teamPlayers);

            return team;
        }

        public async Task<IEnumerable<TeamTournamentsDto>> GetAllTeamsByUserId(int userId)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);

            string sql = $@"
    SELECT uteam.id, uteam.name, utourn.id, utourn.name
	FROM team_tactics.user_team as uteam 
	JOIN team_tactics.user_tournament as utourn
		ON utourn.id = uteam.user_tournament_id
	WHERE uteam.user_account_id = @UserId";

            var tourneyTeamsResults = await _dbConnection.QueryAsync<(int teamId, string teamName, int tourneyId, string tourneyName)>(sql);

            return tourneyTeamsResults.Any() ? tourneyTeamsResults.Select(tt => new TeamTournamentsDto(tt.teamId, tt.teamName, tt.tourneyId, tt.tourneyName)) : new List<TeamTournamentsDto>();
        }

        public async Task<IEnumerable<TeamPlayerDto>> GetTeamPlayersByTeamIdAsync(int teamId)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();
            var parameters = new DynamicParameters();
            parameters.Add("TeamId", teamId);

            string sql = $@"
            SELECT 
                p.id,
                p.first_name,
                p.last_name,
                tp.captain,
	            c.id as club_id,
                c.name as club_name,
                pos.id,
                pos.name
            FROM 
                team_tactics.player_user_team tp
            INNER JOIN team_tactics.player p ON tp.player_id = p.id
            INNER JOIN team_tactics.player_contract pc ON p.id = pc.player_id and pc.active = true
            INNER JOIN team_tactics.club c ON pc.club_id = c.id
            INNER JOIN team_tactics.player_position pos ON p.player_position_id = pos.id
            WHERE 
                tp.user_team_id = @TeamId";
            var result = await _dbConnection.QueryAsync<TeamPlayerDto>(sql, parameters);
            return result;

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

        private async Task<DateOnly?> GetTeamLockedDate(int teamId)
        {
            string sql = "SELECT t.locked_date FROM team_tactics.user_team as t WHERE t.id = @TeamId";
            var parameters = new DynamicParameters();
            parameters.Add("TeamId", teamId);
            var lockedDate = await _dbConnection.QuerySingleOrDefaultAsync<DateOnly?>(sql, parameters);
            return lockedDate;
        }

        private async Task<int> UpsertAsync(Team team)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            using var transaction = _dbConnection.BeginTransaction();

            try
            {
                DateOnly? lockedDate = null;

                if (team.Id > 0)
                    lockedDate = await GetTeamLockedDate(team.Id);

                if (team.Status == TeamStatus.Locked && !lockedDate.HasValue)
                {
                    lockedDate = DateOnly.FromDateTime(DateTime.UtcNow);
                }

                var parameters = new DynamicParameters();
                string teamSql;
                if (team.Id == 0)
                {
                    teamSql = @"
    INSERT INTO team_tactics.user_team
        (name, status, locked_date, formation, user_account_id, user_tournament_id)
    VALUES
        (@Name, @Status, @LockedDate, @Formation, @UserId, @TournamentId)
    RETURNING id";
                }
                else
                {
                    parameters.Add("Id", team.Id);
                    teamSql = @"
    UPDATE team_tactics.user_team SET
        name = @Name,
        status = @Status,
        locked_date = @LockedDate,
        formation = @Formation,
        user_account_id = @UserId,
        user_tournament_id = @TournamentId
    WHERE
        id = @Id
    RETURNING id";
                }

                parameters.Add("Name", team.Name);
                parameters.Add("Status", (int)team.Status);
                parameters.Add("LockedDate", lockedDate);
                parameters.Add("Formation", team.Formation);
                parameters.Add("UserId", team.UserId);
                parameters.Add("TournamentId", team.TournamentId);

                int teamId = await _dbConnection.QuerySingleOrDefaultAsync<int>(teamSql, parameters, transaction);
                if (teamId == 0 && team.Id > 0)
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

                if (team.Players.Any())
                {
                    var playerValues = new List<string>();
                    var playerParameters = new DynamicParameters();

                    int i = 0;
                    foreach (var player in team.Players)
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
                team.SetId(teamId);
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
