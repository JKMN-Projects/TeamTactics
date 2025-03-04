using TeamTactics.Domain.Players;
using TeamTactics.Domain.Teams;

namespace TeamTactics.Fixtures
{
    public class TeamFaker : Faker<Team>
    {
        public TeamFaker(int playerCount = 5, int enrollmentCount = 2)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(playerCount, 0, nameof(playerCount));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(playerCount, 11, nameof(playerCount));

            Faker faker = new Faker();
            int userId = faker.Random.Int(1, 100);
            int competitionId = faker.Random.Int(1, 100);

            CustomInstantiator(f => new Team(f.Company.CompanyName(), userId, competitionId));
            RuleFor(x => x.Status, TeamStatus.Draft);
            FinishWith((f, t) =>
            {
                AddStartingPlayers(f, t, playerCount);
                AddEnrollments(f, t, enrollmentCount);
            });

        }

        private static void AddStartingPlayers(Faker f, Team t, int playerCount)
        {
            if (playerCount == 0) return;

            int captainIndex = 0; // f.Random.Int(0, playerCount - 1);
            for (int i = 0; i < playerCount; i++)
            {
                Player player = new PlayerFaker()
                    .RuleFor(p => p.Id, i)
                    .Generate();
                t.AddPlayer(player);
                if (i == captainIndex)
                {
                    t.SetCaptain(player.Id);
                }
            }
        }

        private static void AddEnrollments(Faker f, Team t, int enrollmentCount)
        {
            if (enrollmentCount == 0) return;

            for (int i = 0; i < enrollmentCount; i++)
            {
                t.EnrollInTournament(i);
            }
        }
    }
}
