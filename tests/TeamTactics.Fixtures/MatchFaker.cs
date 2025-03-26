
using TeamTactics.Domain.Clubs;
using TeamTactics.Domain.Competitions;
using TeamTactics.Domain.Matches;

namespace TeamTactics.Fixtures
{
    public sealed class MatchFaker : Faker<Match>
    {
        public MatchFaker(int competitionId, Club homeTeam, Club awayTeam)
        {
            Faker faker = new Faker();
            CustomInstantiator(f => new Match(
                homeTeam.Id,
                awayTeam.Id,
                faker.Random.Int(0, 3),
                faker.Random.Int(0, 3),
                competitionId,
                faker.Date.Past()
            ));
        }

        public static IEnumerable<Match> GenerateMatchesFromCompetition(Competition competition, IEnumerable<Club> clubs)
        {
            List<Match> matches = new List<Match>();
            foreach (Club homeTeam in clubs)
            {
                foreach (Club awayTeam in clubs)
                {
                    if (homeTeam != awayTeam)
                    {
                        matches.Add(new MatchFaker(competition.Id, homeTeam, awayTeam).Generate());
                    }
                }
            }
            return matches;
        }
    }
}
