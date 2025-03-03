
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Domain.Users;

namespace TeamTactics.Application.Users;

public interface IUserRepository : IRepository<User, int>
{
    public Task<int> InsertAsync(User user, string passwordHash);
    public Task UpdateAsync(User user);
    public Task RemoveAsync(User user);

    public Task<bool> CheckPasswordAsync(string passwordHash);
    public Task<User?> FindByEmail(string email);
    public Task<ProfileDto> GetProfileAsync(int id);
}
