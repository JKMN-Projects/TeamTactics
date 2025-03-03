
namespace TeamTactics.Application.Common.Models
{
    public class AuthenticationToken
    {
        public string Token { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }

        public AuthenticationToken(string token, string tokenType, int expiresIn)
        {
            Token = token;
            TokenType = tokenType;
            ExpiresIn = expiresIn;
        }
    }
}
