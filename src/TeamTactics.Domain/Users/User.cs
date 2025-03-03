
namespace TeamTactics.Domain.Users
{
    public class User
    {
        public int Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }

        public User(string username, string email)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(username);
            ArgumentException.ThrowIfNullOrWhiteSpace(email);
            if (email.Contains('@') is false) throw new ArgumentException("Must be a valid e-mail adress", nameof(email));

            Username = username;
            Email = email;
        }
    }
}
