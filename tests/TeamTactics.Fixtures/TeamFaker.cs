using TeamTactics.Domain.Players;
using TeamTactics.Domain.Teams;

namespace TeamTactics.Fixtures
{
    public class TeamFaker : Faker<Team>
    {
        public TeamFaker(int playerCount = 5, IEnumerable<Player>? players = null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(playerCount, 0, nameof(playerCount));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(playerCount, 11, nameof(playerCount));

            Faker faker = new Faker();
            int userId = faker.Random.Int(1, 100);
            int tournamentId = faker.Random.Int(1, 100);

            CustomInstantiator(f => new Team(f.Company.CompanyName(), userId, tournamentId));

            players ??= GeneratePlayers(playerCount);

            FinishWith((f, t) =>
            {
                if (playerCount == 0) return;

                foreach (var player in players)
                {
                    t.AddPlayer(player);
                }
                t.SetCaptain(players.ElementAt(0).Id);
            });
        }

        private static IEnumerable<Player> GeneratePlayers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new PlayerFaker()
                    .RuleFor(p => p.Id, i)
                    .Generate();
            }
        }
    }
}
