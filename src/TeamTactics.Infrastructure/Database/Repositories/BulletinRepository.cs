using System;
using System.Collections.Generic;
using TeamTactics.Application.Bulletins;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamTactics.Domain.Bulletins;
using Dapper;
using System.Data;
using System.Data.Common;

namespace TeamTactics.Infrastructure.Database.Repositories;

internal class BulletinRepository(IDbConnection dbConnection) : IBulletinRepository
{
    private IDbConnection _dbConnection = dbConnection;

    public Task<IEnumerable<Bulletin>> FindAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Bulletin?> FindById(int id)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"SELECT id, text, created_time, user_tournament_id, user_account_id 
                  FROM bulletin 
                  WHERE id = @Id";

        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        var bulletin = await _dbConnection.QuerySingleOrDefaultAsync<Bulletin?>(sql, parameters);
        
        return bulletin;
    }

    public async Task<IEnumerable<Bulletin>> FindInTournamentAsync(int tournamentId)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"SELECT id, text, created_time, user_tournament_id, user_account_id 
                  FROM bulletin 
                  WHERE user_tournament_id = @TournamentId
                  ORDER BY created_time DESC";

        var parameters = new DynamicParameters();
        parameters.Add("TournamentId", tournamentId);

        var bulletins = await _dbConnection.QueryAsync<Bulletin>(sql, parameters);

        return bulletins;
    }

    public async Task<bool> GetIfBulletinOwner(int userId, int bulletinId)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"SELECT * 
                  FROM bulletin 
                  WHERE id = @BulletinId AND user_account_id = @UserId";

        var parameters = new DynamicParameters();
        parameters.Add("BulletinId", bulletinId);
        parameters.Add("UserId", userId);

        int count = await _dbConnection.ExecuteScalarAsync<int>(sql, parameters);
        return count > 0;
    }

    public async Task<int> InsertAsync(Bulletin bulletin)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"INSERT INTO bulletin (text, created_time, user_tournament_id, user_account_id)
                  VALUES (@Text, @CreatedTime, @TournamentId, @UserId)
                  RETURNING id";

        var parameters = new DynamicParameters();
        parameters.Add("Text", bulletin.Text);
        parameters.Add("CreatedTime", bulletin.CreatedTime);
        parameters.Add("TournamentId", bulletin.TournamentId);
        parameters.Add("UserId", bulletin.UserId);

        int newId = await _dbConnection.ExecuteScalarAsync<int>(sql, parameters);
        return newId;
    }

    public async Task RemoveAsync(int id)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"DELETE FROM bulletin WHERE id = @Id";

        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        await _dbConnection.ExecuteAsync(sql, parameters);
    }

    public async Task UpdateAsync(Bulletin bulletin)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"UPDATE bulletin 
                  SET text = @Text, 
                      last_edited_time = @LastEditTime
                  WHERE id = @Id";

        var parameters = new DynamicParameters();
        parameters.Add("Id", bulletin.Id);
        parameters.Add("Text", bulletin.Text);
        parameters.Add("LastEditTime", DateTime.UtcNow);

        await _dbConnection.ExecuteAsync(sql, parameters);
    }
}
