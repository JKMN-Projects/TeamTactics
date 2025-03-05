
namespace TeamTactics.Domain.Users
{
    public class User : Entity
    {
        public string Username { get; private set; }
        public string Email { get; private set; }
        public SecurityInfo SecurityInfo { get; private set; }

        public User(string username, string email, SecurityInfo securityInfo)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(username);
            ArgumentException.ThrowIfNullOrWhiteSpace(email);
            if (email.Contains('@') is false || email.Count('@'.Equals) > 1 || email.Contains('.') is false)
                throw new ArgumentException("Must be a valid e-mail adress", nameof(email));

            Username = username;
            Email = email;
            SecurityInfo = securityInfo;
        }
    }

    public sealed record SecurityInfo(string Salt);
}
