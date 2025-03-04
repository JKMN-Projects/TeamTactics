
using System.Data.Common;
using TeamTactics.Application.Users;
using TeamTactics.Domain.Users;
using Dapper;
using System.Data;

namespace TeamTactics.Infrastructure.Database.Repositories;

internal class UserRepository(IDbConnection dbConnection) : IUserRepository
{
    private IDbConnection _dbConnection = dbConnection;

    public Task<bool> CheckPasswordAsync(string passwordHash)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<User>> FindAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<User?> FindByEmail(string email)
    {
        throw new NotImplementedException();
    }

    public Task<User> FindById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<ProfileDto> GetProfileAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetUserSaltAsync(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<int> InsertAsync(User user, string passwordHash)
    {
        throw new NotImplementedException();
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
