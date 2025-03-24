
using TeamTactics.Domain.Bulletins;
using TeamTactics.Domain.Competitions;
using TeamTactics.Domain.Tournaments;
using TeamTactics.Domain.Users;
using TeamTactics.Infrastructure.Database.Repositories;
using TeamTactics.Infrastructure.IntegrationTests.Seeding;

namespace TeamTactics.Infrastructure.IntegrationTests.Repositories
{
    public abstract class BulletinRepositoryTests : RepositoryTestBase
    {
        private readonly BulletinRepository _sut;
        private readonly DataSeeder _dataSeeder;

        public BulletinRepositoryTests(PostgresDatabaseFixture dbFixture) : base(dbFixture)
        {
            _sut = new BulletinRepository(DbConnection);
            _dataSeeder = new DataSeeder(DbConnection);
        }

        public sealed class InsertAsync : BulletinRepositoryTests
        {
            public InsertAsync(PostgresDatabaseFixture dbFixture) : base(dbFixture)
            {
            }

            [Fact]
            public async Task Should_SaveBulletinToDb_When_InsertIsSuccesful()
            {
                // Arrange
                User user = await _dataSeeder.SeedRandomUserAsync();
                Competition competition = await _dataSeeder.SeedRandomCompetitionAsync();
                Tournament tournament = new Tournament("name", user.Id, competition.Id);
                var tournamentRepository = new TournamentRepository(DbConnection);
                int tournamentId = await tournamentRepository.InsertAsync(tournament);
                var bulletin = new Bulletin("Test", DateTime.UtcNow, tournamentId, user.Id);

                // Act
                int id = await _sut.InsertAsync(bulletin);

                // Assert
                Bulletin? insertedBulletin = await _sut.FindByIdAsync(id);
                Assert.NotNull(insertedBulletin);
                Assert.Equal(bulletin.Text, insertedBulletin!.Text);
                Assert.Equal(bulletin.CreatedTime.Date, insertedBulletin.CreatedTime.Date);
                Assert.Equal(bulletin.TournamentId, insertedBulletin.TournamentId);
                Assert.Equal(bulletin.UserId, insertedBulletin.UserId);
            }
        }
    }
}
