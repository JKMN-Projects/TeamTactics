﻿
using System.Data.Common;
using TeamTactics.Application.Users;
using TeamTactics.Domain.Users;
using Dapper;
using System.Data;
using TeamTactics.Application.Common.Exceptions;

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

    public async Task<bool> CheckIfEmailExistsAsync(string email)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"SELECT id FROM team_tactics.user_account WHERE email = @Email";

        var parameters = new DynamicParameters();
        parameters.Add("Email", email);

        int? id = await _dbConnection.QuerySingleOrDefaultAsync<int?>(sql, parameters);

        return id != null;
    }

    public async Task<bool> CheckIfUsernameExistsAsync(string username)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"SELECT id FROM team_tactics.user_account WHERE username = @Username";

        var parameters = new DynamicParameters();
        parameters.Add("Username", username);

        int? id = await _dbConnection.QuerySingleOrDefaultAsync<int?>(sql, parameters);

        return id != null;
    }

    public Task<IEnumerable<User>> FindAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<User?> FindByEmailOrUsername(string emailOrUsername)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"SELECT id, username, email FROM team_tactics.user_account
	                    WHERE email = @EmailOrUsername OR username = @EmailOrUsername";

        var parameters = new DynamicParameters();
        parameters.Add("EmailOrUsername", emailOrUsername);

        var userResult = await _dbConnection.QuerySingleOrDefaultAsync<(int id, string username, string email)?>(sql, parameters);

        return userResult.HasValue ? new User(userResult.Value.id, userResult.Value.username, userResult.Value.email) : null;
    }

    public async Task<User?> FindByIdAsync(int id)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"SELECT id, username, email FROM team_tactics.user_account WHERE id = @ID";

        var parameters = new DynamicParameters();
        parameters.Add("ID", id);

        var userResult = await _dbConnection.QuerySingleOrDefaultAsync<(int id, string username, string email)?>(sql, parameters);

        return userResult.HasValue ? new User(userResult.Value.id, userResult.Value.username, userResult.Value.email) : null;
    }

    public async Task<ProfileDto> GetProfileAsync(int id)
    {
        //Get associated tournaments from TournamentRepo

        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        var userParams = new DynamicParameters();
        userParams.Add("UserId", id);

        // Get user account info
        string userSql = @"
    SELECT id, username, email
    FROM team_tactics.user_account
    WHERE id = @UserId";

        var userInfo = await _dbConnection.QuerySingleOrDefaultAsync<(int userId, string Username, string Email)>(userSql, userParams);

        if (userInfo == default)
            throw new EntityNotFoundException("Profile", id); // User not found

        // Create and return the DTO
        return new ProfileDto(userInfo.userId, userInfo.Username, userInfo.Email);
    }



    public async Task<string?> GetUserSaltAsync(int userId)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"SELECT salt FROM team_tactics.user_account WHERE id = @UserId";

        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);

        string? userSalt = await _dbConnection.QuerySingleOrDefaultAsync<string?>(sql, parameters);

        return userSalt;
    }

    public async Task<User> InsertAsync(User user, string passwordHash, string salt)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"
    INSERT INTO team_tactics.user_account (username, email, password_hash, salt) 
    VALUES (@Username, @Email, @PasswordHash, @Salt)
    RETURNING id";

        var parameters = new DynamicParameters();
        parameters.Add("Username", user.Username);
        parameters.Add("Email", user.Email);
        parameters.Add("PasswordHash", passwordHash);
        parameters.Add("Salt", salt);

        // Execute query and get the generated ID
        int userId = await _dbConnection.QuerySingleAsync<int>(sql, parameters);

        user.SetId(userId);

        return user;
    }

    public async Task RemoveAsync(User user)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        var parameters = new DynamicParameters();
        parameters.Add("Id", user.Id);

        //ON DELETE CASCADE deletes all player_user_team associated with the team
        string sql = @"DELETE FROM team_tactics.user_account WHERE id = @Id";

        int rowsAffected = await _dbConnection.ExecuteAsync(sql, parameters);
        if (rowsAffected == 0) 
        {
            throw EntityNotFoundException.ForEntity<User>(user.Id, nameof(User.Id));
        }
    }

    public Task UpdateAsync(User user)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateInfoAsync(int userId, string username, string email)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"
    UPDATE team_tactics.user_account SET
        username = @Username, 
        email = @Email
    WHERE id = @UserId";

        var parameters = new DynamicParameters();
        parameters.Add("Username", username);
        parameters.Add("Email", email);
        parameters.Add("UserId", userId);

        await _dbConnection.ExecuteAsync(sql, parameters);
    }

    public async Task UpdateSecurityAsync(int userId, string passwordHash, string salt)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"
    UPDATE team_tactics.user_account SET
        password_hash = @PasswordHash, 
        salt = @Salt
    WHERE id = @UserId";

        var parameters = new DynamicParameters();
        parameters.Add("PasswordHash", passwordHash);
        parameters.Add("Salt", salt);
        parameters.Add("UserId", userId);


        await _dbConnection.ExecuteAsync(sql, parameters);
    }
}
