
using TeamTactics.Domain.Clubs;
using TeamTactics.Domain.Players;

namespace TeamTactics.Fixtures
{
    public sealed class PlayerFaker : Faker<Player>
    {
        public PlayerFaker(
            string? firstname = null,
            string? lastname = null,
            params Club[] clubs)
        {
            Faker faker = new Faker();
            string externalId = new Guid().ToString().Substring(0, 8);
            int positionId = faker.Random.Int(1, 4);

            CustomInstantiator(f => 
                new Player(
                    firstname ?? f.Person.FirstName,
                    lastname ?? f.Person.LastName,
                    DateOnly.FromDateTime(f.Person.DateOfBirth),
                    externalId,
                    positionId)
            );

            List<Club> clubsPlayedAt = clubs.Length > 0 
                ? clubs.ToList()
                : new ClubFaker()
                    .RuleFor(c => c.Id, Faker.GlobalUniqueIndex)
                    .Generate(3);

            FinishWith((f, p) =>
            {
                clubsPlayedAt.ForEach(club => p.SignContract(club.Id));
            });
        }
    }
}
