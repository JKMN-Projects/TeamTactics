﻿using Dapper;
using System.Data;
using System.Data.Common;
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

        public Task<Tournament?> FindById(int id)
        {
            throw new NotImplementedException();
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
        public async Task<bool> CheckTournamentInviteCodeAsync(string inviteCode)
        {
            if (string.IsNullOrWhiteSpace(inviteCode)) return false;

            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            var parameters = new DynamicParameters();
            parameters.Add("InviteCode", inviteCode);

            string sql = @"SELECT id FROM team_tactics.user_tournament WHERE invite_code = @InviteCode";

            var tourneyId = await _dbConnection.QuerySingleOrDefaultAsync<int?>(sql, parameters);

            if (tourneyId == null) return false;

            return true;
        }

        public async Task RemoveAsync(int id)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            var parameters = new DynamicParameters();
            parameters.Add("Id", id);

            //ON DELETE CASCADE deletes all player_user_team associated with the team
            string sql = @"DELETE FROM team_tactics.user_tournament WHERE id = @Id";

            await _dbConnection.ExecuteAsync(sql, parameters);
        }

        public Task UpdateAsync(Tournament tournament)
        {
            throw new NotImplementedException();
        }
    }
}
