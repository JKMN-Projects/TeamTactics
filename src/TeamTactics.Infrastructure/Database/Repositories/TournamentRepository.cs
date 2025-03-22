using Dapper;
using System.Data;
using System.Data.Common;
using TeamTactics.Application.Common.Exceptions;
using TeamTactics.Application.Tournaments;
using TeamTactics.Domain.Tournaments;
using TeamTactics.Domain.Users;

namespace TeamTactics.Infrastructure.Database.Repositories
{
    class TournamentRepository(IDbConnection dbConnection) : ITournamentRepository
    {
        private IDbConnection _dbConnection = dbConnection;

        public Task<IEnumerable<Tournament>> FindAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Tournament?> FindByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<TournamentDto?> GetTournamentDtoByIdAsync(int tournamentId)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            string sql = @$"
        SELECT ut.id, ut.name, ut.description, ut.invite_code, c.name, ua.username, ua.id
	FROM team_tactics.user_tournament as ut 
	JOIN team_tactics.competition as c 
		ON c.id  = ut.competition_id
	JOIN team_tactics.user_account as ua 
		ON ua.id = ut.user_account_id
	WHERE ut.id = @TournamentId";

            var parameters = new DynamicParameters();
            parameters.Add("TournamentId", tournamentId);

            var result = await _dbConnection.QuerySingleOrDefaultAsync<(int Id, string Name, string Description, string inviteCode, string competitionName, string ownerUsername, int ownerUserId)>(sql, parameters);

            return new TournamentDto(result.Id, result.Name, result.Description, result.inviteCode, result.competitionName, result.ownerUsername, result.ownerUserId);
        }

        public async Task<int> InsertAsync(Tournament tournament)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            // SQL for inserting a single user with password hash
            string sql = @"
    INSERT INTO team_tactics.user_tournament (name, description, user_account_id, competition_id) 
    VALUES (@Name, @Description, @UserId, @CompId)
    RETURNING id, invite_code"
            ;

            var parameters = new DynamicParameters();
            parameters.Add("Name", tournament.Name);
            parameters.Add("Description", tournament.Description);
            parameters.Add("UserId", tournament.CreatedByUserId);
            parameters.Add("CompId", tournament.CompetitionId);

            // Execute query and get both the generated ID and invite_code
            var result = await _dbConnection.QuerySingleAsync<(int Id, string InviteCode)>(sql, parameters);

            // Update the tournament object with the new values
            tournament.SetId(result.Id);
            tournament.SetInviteCode(result.InviteCode);

            return tournament.Id;
        }

        /// <summary>
        /// Should call Team Insert after, to create an empty team to join
        /// </summary>
        /// <param name="inviteCode"></param>
        /// <returns></returns>
        public async Task<int?> FindIdByInviteCodeAsync(string inviteCode)
        {
            if (string.IsNullOrWhiteSpace(inviteCode)) return null;

            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            var parameters = new DynamicParameters();
            parameters.Add("InviteCode", inviteCode);

            string sql = @"SELECT id FROM team_tactics.user_tournament WHERE invite_code = @InviteCode";

            var tourneyId = await _dbConnection.QuerySingleOrDefaultAsync<int?>(sql, parameters);

            return tourneyId;
        }

        public async Task<IEnumerable<Tournament>> GetOwnedTournamentsAsync(int ownerId)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            string sql = @"SELECT id, name, description, user_account_id, competition_id, invite_code FROM team_tactics.user_tournament WHERE user_account_id = @OwnerId";

            var parameters = new DynamicParameters();
            parameters.Add("OwnerId", ownerId);

            var result = await _dbConnection.QueryAsync<(int Id, string Name, string Description, int UserId, int CompId, string InviteCode)>(sql, parameters);

            return result.Any()
                ? result.Select(r => {
                    var t = new Tournament(r.Name, r.UserId, r.CompId, r.Description, r.InviteCode);
                    t.SetId(r.Id);
                    t.SetInviteCode(r.InviteCode);
                    return t;
                })
                : new List<Tournament>();
        }

        public async Task<IEnumerable<Tournament>> GetJoinedTournamentsAsync(string userId)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            string sql = @"
                  SELECT ut.id, ut.name, ut.description, ut.user_account_id, ut.competition_id, ut.invite_code 
                    FROM team_tactics.user_tournament AS ut
                    JOIN team_tactics.user_team AS team
                        ON ut.id = team.user_tournament_id
                    WHERE team.user_account_id = @UserId";

            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);

            var result = await _dbConnection.QueryAsync<(int Id, string Name, string Description, int UserId, int CompId, string InviteCode)>(sql, parameters);

            return result.Any()
                ? result.Select(r => {
                    var t = new Tournament(r.Name, r.UserId, r.CompId, r.Description, r.InviteCode);
                    t.SetId(r.Id);
                    t.SetInviteCode(r.InviteCode);
                    return t;
                })
                : new List<Tournament>();
        }

        /// <summary>
        /// Get Totalpoints with TeamId from Point Repo
        /// </summary>
        /// <param name="tournamentId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TournamentTeamDto>> GetTeamsInTournamentAsync(int tournamentId)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            string sql = @$"
        SELECT 
            team.id, 
            team.name, 
        FROM team_tactics.user_team team
        WHERE team.user_tournament_id = @TournamentId
        ORDER BY team.name";

            var parameters = new DynamicParameters();
            parameters.Add("TournamentId", tournamentId);

            var tournamentTeamsResult = await _dbConnection.QueryAsync<(int teamId, string teamName)>(sql, parameters);

            return tournamentTeamsResult.Any() ? tournamentTeamsResult.Select(tt => new TournamentTeamDto(tt.teamId, tt.teamName)) : new List<TournamentTeamDto>();
        }
        
        public async Task RemoveAsync(int id)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            var parameters = new DynamicParameters();
            parameters.Add("Id", id);

            //ON DELETE CASCADE deletes all player_user_team associated with the team
            string sql = @"DELETE FROM team_tactics.user_tournament WHERE id = @Id";

            int rowsAffected = await _dbConnection.ExecuteAsync(sql, parameters);

            if (rowsAffected == 0)
                throw EntityNotFoundException.ForEntity<Tournament>(id, nameof(Tournament.Id));
        }

        public async Task UpdateAsync(Tournament tournament)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            var parameters = new DynamicParameters();
            parameters.Add("Name", tournament.Name);
            parameters.Add("Description", tournament.Description);
            parameters.Add("TourneyId", tournament.Id);

            //ON DELETE CASCADE deletes all player_user_team associated with the team
            string sql = @"UPDATE team_tactics.user_tournament SET
                                name = @Name, 
                                description = @Description
                            WHERE id = @TourneyId";

            int rowsAffected = await _dbConnection.ExecuteAsync(sql, parameters);

            if (rowsAffected == 0)
                throw EntityNotFoundException.ForEntity<Tournament>(tournament.Id, nameof(Tournament.Id));
        }

        public async Task UpdateOwnerAsync(int tournamentId, int previousOwnerId, int newOwnerId)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            var parameters = new DynamicParameters();
            parameters.Add("TourneyId", tournamentId);
            parameters.Add("PreviousOwnerId", previousOwnerId);
            parameters.Add("NewOwnerId", newOwnerId);

            //ON DELETE CASCADE deletes all player_user_team associated with the team
            string sql = @"UPDATE team_tactics.user_tournament SET
                                user_account_id = @NewOwnerId
                            WHERE id = @TourneyId AND user_account_id = @PreviousOwnerId";

            int rowsAffected = await _dbConnection.ExecuteAsync(sql, parameters);

            if (rowsAffected == 0)
                throw EntityNotFoundException.ForEntity<Tournament>(tournamentId + " | " + previousOwnerId, "TournamentId and UserAccountId");
        }
    }
}
