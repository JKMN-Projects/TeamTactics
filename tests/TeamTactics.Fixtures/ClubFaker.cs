
using TeamTactics.Domain.Clubs;

namespace TeamTactics.Fixtures;

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
        Faker faker = new Faker();
        string externalId = new Guid().ToString().Substring(0, 8);
        var getTeamName = () => teamName ?? faker.PickRandom(_teamNames);


        CustomInstantiator(f => new Club(getTeamName(), externalId));
    }
}
