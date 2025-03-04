
using TeamTactics.Application.Users;
using TeamTactics.Domain.Users;

namespace TeamTactics.Infrastructure.Database.Repositories;

internal class UserRepository : IUserRepository
{
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
