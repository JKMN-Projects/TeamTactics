
namespace TeamTactics.Domain.Users
{
    public class User
    {
        public int Id { get; private set; }
        public string UserName { get; private set; }
        public string Email { get; private set; }

        public User(string userName, string email)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userName);
            ArgumentException.ThrowIfNullOrWhiteSpace(email);
            if (email.Contains('@') is false || email.Count('@'.Equals) > 1 || email.Contains('.') is false) 
                throw new ArgumentException("Must be a valid e-mail adress", nameof(email));

            UserName = userName;
            Email = email;
        }
    }
}
