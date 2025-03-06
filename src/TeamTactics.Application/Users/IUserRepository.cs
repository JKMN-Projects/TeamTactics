
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Domain.Users;

namespace TeamTactics.Application.Users;

public interface IUserRepository : IRepository<User, int>
{
    public Task<User> InsertAsync(User user, string passwordHash);
    public Task UpdateAsync(User user);
    public Task RemoveAsync(User user);

    public Task<bool> CheckPasswordAsync(string emailOrUsername, string passwordHash);
    public Task<bool> CheckIfEmailExistsAsync(string email);
    public Task<bool> CheckIfUsernameExistsAsync(string username);
    

    public Task<User?> FindByEmail(string email);
    public Task<string?> GetUserSaltAsync(int userId);
    public Task<ProfileDto> GetProfileAsync(int id);
    public Task UpdateSecurityAsync(int userId, string passwordHash, string salt);
    public Task UpdateInfoAsync(int userId, string username, string email);
}
