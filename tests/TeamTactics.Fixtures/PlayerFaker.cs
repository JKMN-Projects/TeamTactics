
using TeamTactics.Domain.Players;

namespace TeamTactics.Fixtures
{
    public sealed class PlayerFaker : Faker<Player>
    {
        public PlayerFaker(int playerContracts = 3)
        {
            Faker faker = new Faker();
            string externalId = new Guid().ToString().Substring(0, 8);
            int positionId = faker.Random.Int(1, 100);

            CustomInstantiator(f => new Player(f.Person.FirstName, f.Person.LastName, DateOnly.FromDateTime(f.Person.DateOfBirth), externalId, positionId));

            int playerId = faker.UniqueIndex;
            RuleFor(x => x.Id, playerId);

            FinishWith((f, p) =>
            {
                for (int i = 0; i < playerContracts; i++)
                {
                    int clubId = faker.Random.Int(1, 100);
                    p.SignContract(clubId);
                }
            });
        }
    }
}
