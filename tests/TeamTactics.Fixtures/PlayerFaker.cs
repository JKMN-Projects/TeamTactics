
using TeamTactics.Domain.Clubs;
using TeamTactics.Domain.Players;

namespace TeamTactics.Fixtures
{
    public sealed class PlayerFaker : Faker<Player>
    {
        public PlayerFaker(int playerContracts = 3, IEnumerable<Club>? clubs = null)
        {
            Faker faker = new Faker();
            string externalId = new Guid().ToString().Substring(0, 8);
            int positionId = faker.Random.Int(1, 4);

            CustomInstantiator(f => new Player(f.Person.FirstName, f.Person.LastName, DateOnly.FromDateTime(f.Person.DateOfBirth), externalId, positionId));

            clubs ??= new ClubFaker()
                .RuleFor(c => c.Id, Faker.GlobalUniqueIndex)
                .Generate(3);

            FinishWith((f, p) =>
            {
                foreach (var club in clubs)
                {
                    p.SignContract(club.Id);
                }
            });
        }
    }
}
