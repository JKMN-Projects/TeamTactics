
using TeamTactics.Domain.Users;

namespace TeamTactics.Fixtures
{
    public class UserFaker : Faker<User>
    {
        public UserFaker()
        {
            CustomInstantiator(f => new User(
                f.Internet.UserName(),
                f.Internet.Email(),
                new SecurityInfo(Convert.ToBase64String(f.Random.Bytes(32)))
            ));
        }
    }
}
