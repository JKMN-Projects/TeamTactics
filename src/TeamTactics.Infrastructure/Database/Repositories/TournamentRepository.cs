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

        public Task<Tournament?> FindByIdAsync(int id)
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

        public Task RemoveAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Tournament model)
        {
            throw new NotImplementedException();
        }
    }
}
