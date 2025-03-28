﻿using TeamTactics.Application.Common.Exceptions;
using TeamTactics.Domain.Users;
using TeamTactics.Infrastructure.Database.Repositories;

namespace TeamTactics.Infrastructure.IntegrationTests.Repositories
{
    public abstract class UserRepositoryTests : RepositoryTestBase, IAsyncLifetime
    {
        private readonly UserRepository _sut;

        protected UserRepositoryTests(PostgresDatabaseFixture factory) : base(factory)
        {
            _sut = new UserRepository(DbConnection);
        }

        public override async Task DisposeAsync()
        {
            await base.DisposeAsync();
            await ResetDatabaseAsync();
        }

        public override Task InitializeAsync() => base.InitializeAsync();

        public sealed class InsertAsync : UserRepositoryTests
        {
            public InsertAsync(PostgresDatabaseFixture factory) : base(factory)
            {
            }

            [Fact]
            public async Task Should_InsertUser()
            {
                // Arrange
                var user = new User("username", "email@jknm.com");

                // Act
                User result = await _sut.InsertAsync(user, "passwordHash", "salt");
                
                // Assert
                var addedUser = await _sut.FindByIdAsync(result.Id);
                Assert.NotNull(addedUser);
                Assert.Equal(user.Username, addedUser.Username);
                Assert.Equal(user.Email, addedUser.Email);
            }
        }

        public sealed class RemoveAsync : UserRepositoryTests
        {
            public RemoveAsync(PostgresDatabaseFixture factory) : base(factory)
            {
            }

            [Fact]
            public async Task Should_RemoveUser()
            {
                // Arrange
                var user = new User("username", "email@jknm.com");
                User addedUser = await _sut.InsertAsync(user, "passwordHash", "salt");

                // Act  
                await _sut.RemoveAsync(addedUser);

                // Assert
                var removedUser = await _sut.FindByIdAsync(addedUser.Id);
                Assert.Null(removedUser);
            }

            [Fact]
            public async Task Should_ThrowEntityNotFoundException_WhenUserDoesNotExist()
            {
                // Arrange
                var user = new User("username", "email@jknm.com");

                // Act
                var act = async () => await _sut.RemoveAsync(user);

                // Assert
                await Assert.ThrowsAsync<EntityNotFoundException>(act);
            }
        }

        public sealed class CheckPasswordAsync : UserRepositoryTests
        {
            public CheckPasswordAsync(PostgresDatabaseFixture factory) : base(factory)
            {
            }

            [Fact]
            public async Task Should_ReturnTrue_WhenPasswordIsCorrect()
            {
                // Arrange
                var user = new User("username", "email@jknm.com");
                User addedUser = await _sut.InsertAsync(user, "passwordHash", "salt");
                
                // Act
                bool result = await _sut.CheckPasswordAsync(addedUser.Email, "passwordHash");

                // Assert
                Assert.True(result);
            }

            [Fact]

            public async Task Should_ReturnFalse_WhenPasswordIsIncorrect()
            {
                // Arrange
                var user = new User("username", "email@jknm.com");
                User addedUser = await _sut.InsertAsync(user, "passwordHash", "salt");

                // Act
                bool result = await _sut.CheckPasswordAsync(addedUser.Email, "anotherPasswordHash");

                // Assert
                Assert.False(result);
            }
        }

        public sealed class CheckIfEmailExistsAsync : UserRepositoryTests
        {
            public CheckIfEmailExistsAsync(PostgresDatabaseFixture factory) : base(factory)
            {
            }

            [Fact]
            public async Task Should_ReturnTrue_WhenEmailExists()
            {
                // Arrange
                var user = new User("username", "email@jknm.com");
                User addedUser = await _sut.InsertAsync(user, "passwordHash", "salt");

                // Act
                bool result = await _sut.CheckIfEmailExistsAsync(addedUser.Email);

                // Assert
                Assert.True(result);
            }
        }

        public sealed class CheckIfUsernameExistsAsync : UserRepositoryTests
        {
            public CheckIfUsernameExistsAsync(PostgresDatabaseFixture factory) : base(factory)
            {
            }

            [Fact]
            public async Task Should_ReturnTrue_WheUsernameExists()
            {
                // Arrange
                var user = new User("username", "email@jknm.com");
                User addedUser = await _sut.InsertAsync(user, "passwordHash", "salt");

                // Act
                bool result = await _sut.CheckIfUsernameExistsAsync(addedUser.Username);

                // Assert
                Assert.True(result);
            }
        }
    }
}
