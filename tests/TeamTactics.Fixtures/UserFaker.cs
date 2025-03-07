
using TeamTactics.Domain.Users;

namespace TeamTactics.Fixtures
{
    public class UserFaker : Faker<User>
    {
        public UserFaker()
        {
            CustomInstantiator(f => new User(
                f.Internet.UserName(),
                f.Internet.Email())
            );
        }
    }
}
