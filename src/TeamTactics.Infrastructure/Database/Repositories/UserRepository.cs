
using System.Data.Common;
using TeamTactics.Application.Users;
using TeamTactics.Domain.Users;
using Dapper;
using System.Data;

namespace TeamTactics.Infrastructure.Database.Repositories;

internal class UserRepository(IDbConnection dbConnection) : IUserRepository
{
    private IDbConnection _dbConnection = dbConnection;

    public async Task<bool> CheckPasswordAsync(string emailOrUsername, string passwordHash)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        // SQL to find user by either email or username
        string sql = @"
    SELECT password_hash 
    FROM team_tactics.user_account
    WHERE email = @EmailOrUsername OR username = @EmailOrUsername";

        var parameters = new DynamicParameters();
        parameters.Add("EmailOrUsername", emailOrUsername);

        // Get stored password hash from database
        string? storedHash = await _dbConnection.QuerySingleOrDefaultAsync<string?>(sql, parameters);

        // Return true if password matches (hash exists and equals provided hash)
        return storedHash != null && storedHash == passwordHash;
    }

    public Task<IEnumerable<User>> FindAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<User?> FindByEmail(string email)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        // SQL for inserting a single user with password hash
        string sql = @"SELECT * FROM team_tactics.user_account
	WHERE email = @Email";

        var parameters = new DynamicParameters();
        parameters.Add("Email", email);

        // Execute query and get the generated ID
        User? user = await _dbConnection.QuerySingleOrDefaultAsync<User?>(sql, parameters);

        return user;
    }

    public async Task<User?> FindById(int id)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        // SQL for inserting a single user with password hash
        string sql = @"SELECT * FROM team_tactics.user_account
	WHERE id = @ID";

        var parameters = new DynamicParameters();
        parameters.Add("ID", id);

        // Execute query and get the generated ID
        User? user = await _dbConnection.QuerySingleOrDefaultAsync<User?>(sql, parameters);

        return user;
    }

    public async Task<ProfileDto> GetProfileAsync(int id)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        var userParams = new DynamicParameters();
        userParams.Add("UserId", id);

        // Get user account info
        string userSql = @"
    SELECT username, email
    FROM team_tactics.user_account
    WHERE id = @UserId";

        var userInfo = await _dbConnection.QuerySingleOrDefaultAsync<(string Username, string Email)>(userSql, userParams);

        if (userInfo == default)
            throw new Application.Common.Exceptions.EntityNotFoundException("Profile", id); // User not found

        // Get tournaments associated with this user
        string tournamentSql = @"
    SELECT t.id, t.name
    FROM team_tactics.user_tournament ut
    JOIN team_tactics.tournament t ON ut.competition_id = t.id
    WHERE ut.user_account_id = @UserId";

        var tournaments = await _dbConnection.QueryAsync<ProfileDto.Tournament>(tournamentSql, userParams);

        // Create and return the DTO
        return new ProfileDto(
            Username: userInfo.Username,
            Email: userInfo.Email,
            Competitions: tournaments.ToList()
        );
    }

    public async Task<string?> GetUserSaltAsync(int userId)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        // SQL for inserting a single user with password hash
        string sql = @"SELECT salt FROM team_tactics.user_account
	WHERE id = @UserId";

        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);

        // Execute query and get the generated ID
        string? userSalt = await _dbConnection.QuerySingleOrDefaultAsync<string?>(sql, parameters);

        return userSalt;
    }

    public async Task<User> InsertAsync(User user, string passwordHash)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        // SQL for inserting a single user with password hash
        string sql = @"
    INSERT INTO team_tactics.user_account (username, email, password_hash, salt) 
    VALUES (@Username, @Email, @PasswordHash, @Salt)
    RETURNING id";

        var parameters = new DynamicParameters();
        parameters.Add("Username", user.Username);
        parameters.Add("Email", user.Email);
        parameters.Add("PasswordHash", passwordHash);
        parameters.Add("Salt", user.SecurityInfo.Salt);

        // Execute query and get the generated ID
        int userId = await _dbConnection.QuerySingleAsync<int>(sql, parameters);

        user.SetId(userId);

        return user;
    }

    public Task RemoveAsync(User user)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(User user)
    {
        throw new NotImplementedException();
    }
}
