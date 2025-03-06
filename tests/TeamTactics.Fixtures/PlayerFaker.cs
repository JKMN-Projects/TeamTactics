
using TeamTactics.Domain.Clubs;
using TeamTactics.Domain.Players;

namespace TeamTactics.Fixtures
{
    public sealed class PlayerFaker : Faker<Player>
    {
        public PlayerFaker(int playerContracts = 3, int clubId = 0)
        {
            Faker faker = new Faker();
            string externalId = new Guid().ToString().Substring(0, 8);
            int positionId = faker.Random.Int(1, 4);

            CustomInstantiator(f => new Player(f.Person.FirstName, f.Person.LastName, DateOnly.FromDateTime(f.Person.DateOfBirth), externalId, positionId));

            FinishWith((f, p) =>
            {
                for (int i = 0; i < playerContracts; i++)
                {
                    int clubIdRangeStart = Faker.GlobalUniqueIndex * 100;

                    int clubId = faker.Random.Int(clubIdRangeStart, clubIdRangeStart + 99);
                    p.SignContract(clubId);
                }
                if (clubId > 0)
                {
                    p.SignContract(clubId);
                }
            });
        }
    }

    public sealed class ClubFaker : Faker<Club>
    {
        private static List<string> _teamNames = [
                "Arsenal",
                "Aston Villa",
                "Brentford",
                "Brighton & Hove Albion",
                "Burnley",
                "Chelsea",
                "Crystal Palace",
                "Everton",
                "Leeds United",
                "Leicester City",
                "Liverpool",
            ];

        public ClubFaker(string? teamName = null)
        {
            string externalId = new Guid().ToString().Substring(0, 8);
            teamName ??= _teamNames[Faker.GlobalUniqueIndex % _teamNames.Count];

            CustomInstantiator(f => new Club(f.Company.CompanyName(), externalId));
        }
    }
}
