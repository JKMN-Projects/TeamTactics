using TeamTactics.Application.Bulletins;
using TeamTactics.Domain.Bulletins;
using Dapper;
using System.Data;

namespace TeamTactics.Infrastructure.Database.Repositories;

internal class BulletinRepository(IDbConnection dbConnection) : IBulletinRepository
{
    private IDbConnection _dbConnection = dbConnection;

    public Task<IEnumerable<Bulletin>> FindAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Bulletin?> FindByIdAsync(int id)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"SELECT id, text, created_time, last_edited_time, user_tournament_id, user_account_id FROM team_tactics.bulletin WHERE id = @Id";

        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        var bulletinsResult = await _dbConnection.QuerySingleOrDefaultAsync<(int id, string text, DateTime createdTime, DateTime lastEditedTime, int tourneyId, int userId)?>(sql, parameters);

        return bulletinsResult.HasValue ? new Bulletin(bulletinsResult.Value.id, bulletinsResult.Value.text, bulletinsResult.Value.createdTime, bulletinsResult.Value.lastEditedTime, bulletinsResult.Value.tourneyId, bulletinsResult.Value.userId) : null;
    }

    public async Task<IEnumerable<BulletinDto>> FindInTournamentAsync(int tournamentId)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"SELECT b.id, b.text, b.created_time, b.last_edited_time, b.user_account_id, ua.username
                  FROM team_tactics.bulletin as b 
                    JOIN team_tactics.user_account as ua
                        ON ua.id = b.user_account_id
                  WHERE b.user_tournament_id = @TournamentId
                  ORDER BY created_time DESC";

        var parameters = new DynamicParameters();
        parameters.Add("TournamentId", tournamentId);

        var bulletinsResult = await _dbConnection.QueryAsync<(int b_id, string b_text, DateTime b_createdTime, DateTime? b_lastEditedTime, int ua_id, string ua_username)>(sql, parameters);

        return bulletinsResult.Select(r => new BulletinDto(r.b_id, r.b_text, r.b_createdTime, r.b_lastEditedTime, r.ua_id, r.ua_username));
    }

    public async Task<bool> GetIfBulletinOwner(int userId, int bulletinId)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"SELECT * FROM team_tactics.bulletin WHERE id = @BulletinId AND user_account_id = @UserId";

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

        string sql = @"INSERT INTO team_tactics.bulletin (text, created_time, user_tournament_id, user_account_id)
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

        string sql = @"DELETE FROM team_tactics.bulletin WHERE id = @Id";

        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        await _dbConnection.ExecuteAsync(sql, parameters);
    }

    public async Task UpdateAsync(Bulletin bulletin)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"
                  UPDATE team_tactics.bulletin SET 
                    text = @Text, 
                    last_edited_time = @LastEditTime
                  WHERE id = @Id";

        var parameters = new DynamicParameters();
        parameters.Add("Id", bulletin.Id);
        parameters.Add("Text", bulletin.Text);
        parameters.Add("LastEditTime", DateTime.UtcNow);

        await _dbConnection.ExecuteAsync(sql, parameters);
    }
}
