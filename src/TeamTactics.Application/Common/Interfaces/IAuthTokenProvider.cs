using System.Diagnostics;
using TeamTactics.Application.Common.Models;
using TeamTactics.Domain.Users;

namespace TeamTactics.Infrastructure.Tokens
{
    public interface IAuthTokenProvider
    {
        public Task<AuthenticationToken> GenerateTokenAsync(User user);
    }
}
