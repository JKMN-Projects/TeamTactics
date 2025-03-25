
using TeamTactics.Domain.Clubs;
using TeamTactics.Domain.Players;
using TeamTactics.Infrastructure.Database.Repositories;
using TeamTactics.Infrastructure.IntegrationTests.Seeding;

namespace TeamTactics.Infrastructure.IntegrationTests.Repositories
{
    public class PlayerRepositoryTests : RepositoryTestBase
    {
        private readonly PlayerRepository _playerRepository;
        private readonly DataSeeder _dataSeeder;

        public PlayerRepositoryTests(PostgresDatabaseFixture dbFixture) : base(dbFixture)
        {
            _playerRepository = new PlayerRepository(DbConnection);
            _dataSeeder = new DataSeeder(DbConnection);
        }

        public sealed class FindByIdAsync : PlayerRepositoryTests
        {
            public FindByIdAsync(PostgresDatabaseFixture dbFixture) : base(dbFixture) { }
            [Fact]
            public async Task Should_ReturnPlayerWithContracts_When_PlayerExists()
            {
                // Arrange
                var clubs = await _dataSeeder.SeedRandomClubsAsync(count: 5);
                Player player = new PlayerFaker(clubs: [..clubs])
                    .Generate();
                await _dataSeeder.SeedPlayerAsync(player);

                // Act
                var result = await _playerRepository.FindByIdAsync(player.Id);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(player.Id, result.Id);
                Assert.Equal(player.FirstName, result.FirstName);
                Assert.Equal(player.LastName, result.LastName);
                Assert.Equal(player.BirthDate, result.BirthDate);
                Assert.Equal(player.ExternalId, result.ExternalId);
                Assert.Equal(player.PositionId, result.PositionId);
                Assert.NotEmpty(result.PlayerContracts);
                Assert.Equal(player.PlayerContracts.Count, result.PlayerContracts.Count);
                Assert.Equal(player.PlayerContracts.Single(c => c.Active).ClubId, result.ActivePlayerContract.ClubId);
            }

            [Fact]
            public async Task Should_ReturnNull_When_PlayerDoesNotExist()
            {
                // Arrange
                var clubs = await _dataSeeder.SeedRandomClubsAsync(count: 5);
                Player player = new PlayerFaker(clubs: [.. clubs])
                    .Generate();
                int playerId = player.Id + 100;

                // Act
                var result = await _playerRepository.FindByIdAsync(playerId);

                // Assert
                Assert.Null(result);
            }
        }
    }
}
