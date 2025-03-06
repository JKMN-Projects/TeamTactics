
using TeamTactics.Domain.Competitions;

namespace TeamTactics.Fixtures
{
    public class CompetitionFaker : Faker<Competition>
    {
        private static List<string> CompetitionNames=
        [
            "Premier League",
            "La Liga",
            "Serie A",
            "Bundesliga",
            "Ligue 1",
            "Superliga",
        ];

        public CompetitionFaker()
        {
            Faker faker = new Faker();
            int year = faker.Date.Past(yearsToGoBack: 10).Year;
            bool isFall = faker.Random.Bool();
            string competitionName = faker.PickRandom(CompetitionNames);

            // Set Start and End Dates for a half season
            DateOnly startDate = new DateOnly(
                year,
                isFall ? 8 : 1,
                1);
            DateOnly endDate = new DateOnly(
                year,
                isFall ? 12 : 5,
                31);

            CustomInstantiator(f => new Competition(
                competitionName,
                startDate,
                endDate
            ));
            RuleFor(c => c.Id, f => f.Random.Int(min: 1));
        }


    }
}
