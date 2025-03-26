using TeamTactics.Domain.Players;
using TeamTactics.Domain.Points;
using TeamTactics.Domain.Teams;
using TeamTactics.Domain.Tournaments;
using TeamTactics.Domain.Users;
using TeamTactics.Infrastructure.Database.Repositories;
using TeamTactics.Infrastructure.IntegrationTests.Seeding;

namespace TeamTactics.Infrastructure.IntegrationTests.Repositories;

public abstract class PointsRepositoryTests : RepositoryTestBase, IAsyncLifetime
{
    private readonly PointRepository _pointsRepository;
    private readonly DataSeeder _dataSeeder;

    protected PointsRepositoryTests(PostgresDatabaseFixture factory) : base(factory)
    {
        _pointsRepository = new PointRepository(DbConnection);
        _dataSeeder = new DataSeeder(DbConnection);
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await ResetDatabaseAsync();
    }

    public override Task InitializeAsync() => base.InitializeAsync();

    private async Task<Team> SeedTeamAsync(int playerCount = 11)
    {
        TeamRepository teamRepository = new TeamRepository(DbConnection);
        User user = await _dataSeeder.SeedRandomUserAsync();
        var seedResult = await _dataSeeder.SeedFullCompetitionAsync();

        var tournamentToInsert = new Tournament("Test Tournament", user.Id, seedResult.Competition.Id, description: "A Tournament description");
        var tournamentRepository = new TournamentRepository(DbConnection);
        int tournamentId = await tournamentRepository.InsertAsync(tournamentToInsert);

        List<Player> teamPlayers = [];
        if (playerCount > 0)
        {
            Faker faker = new Faker();
            var availablePlayers = seedResult.Clubs.SelectMany(c => c.Players.Take(2)).ToList();
            teamPlayers = faker.PickRandom(availablePlayers, playerCount).ToList();
        }

        Team team = new TeamFaker(players: teamPlayers)
            .RuleFor(t => t.UserId, user.Id)
            .RuleFor(t => t.TournamentId, tournamentId)
            .Generate();
        int teamId = await teamRepository.InsertAsync(team);

        return team;
    }

    public sealed class FindAllActiveAsync : PointsRepositoryTests
    {
        public FindAllActiveAsync(PostgresDatabaseFixture factory) : base(factory)
        {
        }

        [Fact]
        public async Task Should_ReturnActivePointsCategories()
        {
            // Arrange
            PointCategory inactivePointCategory = new PointCategory(default, "Test", "Test", 1, false);
            var parameters = new DynamicParameters();
            parameters.Add("Name", inactivePointCategory.Name);
            parameters.Add("Description", inactivePointCategory.Description);
            parameters.Add("PointAmount", inactivePointCategory.PointAmount);
            parameters.Add("Active", inactivePointCategory.Active);
            string insertInactivePointCategorySql = @"
                INSERT INTO team_tactics.point_category (name, description, point_amount, active) 
                VALUES (@Name, @Description, @PointAmount, @Active)";
            await DbConnection.ExecuteAsync(insertInactivePointCategorySql, parameters);

            // Act
            var actual = await _pointsRepository.FindAllActiveAsync();

            // Assert
            Assert.NotEmpty(actual);
            Assert.DoesNotContain(actual, x => x.Name == inactivePointCategory.Name);
        }
    }

    public sealed class FindTeamPointsAsync : PointsRepositoryTests
    {
        public FindTeamPointsAsync(PostgresDatabaseFixture factory) : base(factory)
        {
        }

        [Fact]
        public async Task Should_ReturnTeamPoints()
        {
            // Arrange
            TeamRepository teamRepository = new TeamRepository(DbConnection);
            User user = await _dataSeeder.SeedRandomUserAsync();
            var seedResult = await _dataSeeder.SeedFullCompetitionAsync();
            var matchSeedResult = await _dataSeeder.SeedCompetitionMatchesAsync(seedResult);

            var tournamentToInsert = new Tournament("Test Tournament", user.Id, seedResult.Competition.Id, description: "A Tournament description");
            var tournamentRepository = new TournamentRepository(DbConnection);
            int tournamentId = await tournamentRepository.InsertAsync(tournamentToInsert);

            List<Player> teamPlayers = [];
            Faker faker = new Faker();
            var availablePlayers = seedResult.Clubs.SelectMany(c => c.Players.Take(2)).ToList();
            teamPlayers = faker.PickRandom(availablePlayers, 11).ToList();

            Team team = new TeamFaker(players: teamPlayers)
                .RuleFor(t => t.UserId, user.Id)
                .RuleFor(t => t.TournamentId, tournamentId)
                .Generate();
            team.Lock();
            int teamId = await teamRepository.InsertAsync(team);

            IEnumerable<(int id, string name, int value)> pointCategories = await DbConnection
                .QueryAsync<(int, string, int)>(
                "SELECT id, name, point_amount FROM team_tactics.point_category");
            int expectedPoints = matchSeedResult
                .SelectMany(m => m.PlayerPoints)
                .Where(pp => teamPlayers.Any(tp => tp.Id == pp.PlayerId))
                .Sum(pp => pp.Occurrences * pointCategories.Single(pc => pc.id == pp.PointCategoryId).value);

            // Act
            var actual = await _pointsRepository.FindTeamPointsAsync(team.Id);

            // Assert
            Assert.NotNull(actual);
            Assert.NotEqual(0, actual.TotalPoints); 
            Assert.Equal(expectedPoints, actual.TotalPoints);
        }
    }
}