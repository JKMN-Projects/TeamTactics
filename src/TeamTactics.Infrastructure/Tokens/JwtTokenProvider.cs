
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TeamTactics.Application.Common.Models;
using TeamTactics.Domain.Users;

namespace TeamTactics.Infrastructure.Tokens
{
    internal sealed class JwtTokenProvider : IAuthTokenProvider
    {
        private JwtOptions _jwtOptions;
        private readonly TimeProvider _timeProvider;

        public JwtTokenProvider(IOptionsMonitor<JwtOptions> options, TimeProvider timeProvider)
        {
            _jwtOptions = options.CurrentValue;
            options.OnChange(o => _jwtOptions = o);
            _timeProvider = timeProvider;
        }

        public Task<AuthenticationToken> GenerateTokenAsync(User user)
        {
            byte[] tokenKey = Encoding.UTF8.GetBytes(_jwtOptions.Key);
            DateTimeOffset expireDate = _timeProvider.GetUtcNow().AddMinutes(_jwtOptions.ValidityInMinutes);
            var claims = BuildUserClaims(user);
            SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Audience = _jwtOptions.Url,
                Issuer = _jwtOptions.Url,
                Expires = expireDate.DateTime,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256)
            };
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
            string token = jwtSecurityTokenHandler.WriteToken(securityToken);
            return Task.FromResult(new AuthenticationToken(token, "JWT", _jwtOptions.ValidityInMinutes * 60));
        }

        private static IEnumerable<Claim> BuildUserClaims(User user)
        {
            yield return new Claim(ClaimTypes.NameIdentifier, user.Id.ToString());
            yield return new Claim(ClaimTypes.Name, user.UserName);
            yield return new Claim(ClaimTypes.Email, user.Email);
        }
    }
}
