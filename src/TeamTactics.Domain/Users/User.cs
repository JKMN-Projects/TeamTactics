
namespace TeamTactics.Domain.Users
{
    public class User
    {
        public int Id { get; private set; }
        public string UserName { get; private set; }
        public string Email { get; private set; }
        public SecurityInfo SecurityInfo { get; private set; }

        #region Default Constructor
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private User() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        #endregion

        public User(string userName, string email, SecurityInfo securityInfo)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userName);
            ArgumentException.ThrowIfNullOrWhiteSpace(email);
            if (email.Contains('@') is false || email.Count('@'.Equals) > 1 || email.Contains('.') is false)
                throw new ArgumentException("Must be a valid e-mail adress", nameof(email));

            UserName = userName;
            Email = email;
            SecurityInfo = securityInfo;
        }
    }

    public sealed record SecurityInfo(string Salt);
}
