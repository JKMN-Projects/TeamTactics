
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using TeamTactics.Application.Common.Exceptions;
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Application.Common.Models;
using TeamTactics.Application.Common.Options;
using TeamTactics.Application.Tournaments;
using TeamTactics.Application.Users;
using TeamTactics.Domain.Users;
using TeamTactics.Fixtures;
using TeamTactics.Infrastructure.Tokens;

namespace TeamTactics.Application.UnitTests
{
    public abstract class UserManagerTests : TestBase
    {
        private readonly IUserRepository _userRepositoryMock;
        private readonly ITournamentRepository _tournamentRepository;
        private readonly IHashingService _hashingServiceMock;
        private readonly PasswordValidator _passwordValidatorMock;
        private readonly IAuthTokenProvider _authTokenProviderMock;
        private readonly ILogger<UserManager> _logger;
        private readonly UserManager _sut;

        public UserManagerTests()
        {
            _userRepositoryMock = Substitute.For<IUserRepository>();
            _tournamentRepository = Substitute.For<ITournamentRepository>();
            _hashingServiceMock = Substitute.For<IHashingService>();
            PasswordSecurityOptions passwordSecurityOptions = new PasswordSecurityOptions();
            IOptions<PasswordSecurityOptions> passwordSecurityOptionsMock = Options.Create(passwordSecurityOptions);
            _passwordValidatorMock = Substitute.For<PasswordValidator>(passwordSecurityOptionsMock);
            _authTokenProviderMock = Substitute.For<IAuthTokenProvider>();
            _logger = Substitute.For<ILogger<UserManager>>();

            _sut = new UserManager(
                _userRepositoryMock,
                _tournamentRepository,
                _hashingServiceMock,
                _passwordValidatorMock,
                _authTokenProviderMock,
                _logger);
        }

        public sealed class CreateUserAsync : UserManagerTests
        {
            [Fact]
            public async Task Should_HashPasswordBeforeInsertingUserInRepository()
            {
                // Arrange
                var faker = new Faker();
                string username = faker.Internet.UserName();
                string email = faker.Internet.Email();
                string password = faker.Internet.Password(prefix: "1aA!");
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                _hashingServiceMock.GenerateSalt().Returns(faker.Random.Bytes(32));
                _hashingServiceMock.Hash(Arg.Any<byte[]>(), Arg.Any<byte[]>()).Returns(faker.Random.Bytes(32));

                var expectedUser = new User(username, email);
                _userRepositoryMock.InsertAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>()).Returns(expectedUser);

                // Act
                await _sut.CreateUserAsync(username, email, password);

                // Assert
                await _userRepositoryMock.Received(1)
                    .InsertAsync(Arg.Any<User>(), Arg.Is<string>(x => x != password), Arg.Any<string>());
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public async Task Should_ThrowArgumentException_When_UserNameIsNullOrWhiteSpace(string? username)
            {
                // Arrange
                string email = new Faker().Internet.Email();
                string password = new Faker().Internet.Password(prefix: "1aA!");

                // Act
                async Task Act() => await _sut.CreateUserAsync(username!, email, password);

                // Assert
                var argEx = await Assert.ThrowsAnyAsync<ArgumentException>(Act);
                Assert.Equal("username", argEx.ParamName);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData("someInvalidEmail")]
            [InlineData("someInvalidEmail.com")]
            [InlineData("some@Invalid@Email.com")]
            public async Task Should_ThrowArgumentException_When_EmailIsInvalid(string? email)
            {
                // Arrange
                string username = new Faker().Internet.UserName();
                string password = new Faker().Internet.Password(prefix: "1aA!");

                // Act
                async Task Act() => await _sut.CreateUserAsync(username, email!, password);

                // Assert
                var argEx = await Assert.ThrowsAnyAsync<ArgumentException>(Act);
                Assert.Equal("email", argEx.ParamName);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public async Task Should_ThrowArgumentException_When_PasswordIsInvalid(string? password)
            {
                // Arrange
                string username = new Faker().Internet.UserName();
                string email = new Faker().Internet.Email();

                // Act
                async Task Act() => await _sut.CreateUserAsync(username, email, password!);

                // Assert
                var argEx = await Assert.ThrowsAnyAsync<ArgumentException>(Act);
                Assert.Equal("password", argEx.ParamName);
            }

            [Theory]
            [InlineData("short")]
            [InlineData("longerpass")]
            [InlineData("LongerPassword")]
            [InlineData("longerp4ssword")]
            [InlineData("longerp@ssword")]
            public async Task Should_ThrowValidationException_When_PasswordIsInvalid(string password)
            {
                // Arrange
                string username = new Faker().Internet.UserName();
                string email = new Faker().Internet.Email();

                // Act
                async Task Act() => await _sut.CreateUserAsync(username, email, password);

                // Assert
                var valEx = await Assert.ThrowsAnyAsync<ValidationException>(Act);
                Assert.True(valEx.Errors.ContainsKey("password"));
            }
        }

        public sealed class GetAuthenticationTokenAsync : UserManagerTests
        {
            [Fact]
            public async Task Should_ReturnAuthenticationToken_When_UserExists()
            {
                // Arrange
                var userFaker = new UserFaker();
                User user = userFaker.Generate();
                _userRepositoryMock.FindByEmailOrUsername(user.Email)
                    .Returns(user);
                _userRepositoryMock.CheckPasswordAsync(Arg.Any<string>(), Arg.Any<string>())
                    .Returns(true);
                _authTokenProviderMock.GenerateTokenAsync(user)
                    .Returns(new AuthenticationToken(_faker.Random.Guid().ToString(), "JWT", 3600));

                // Act
                AuthenticationToken token = await _sut.GetAuthenticationTokenAsync(user.Email, _faker.Internet.Password());

                // Assert
                Assert.NotNull(token);
                Assert.Equal("JWT", token.TokenType);
                Assert.Equal(3600, token.ExpiresIn);
            }

            [Fact]
            public async Task Should_ThrowEntityNotFoundException_When_UserDoesNotExist()
            {
                // Arrange
                string email = new Faker().Internet.Email();
                _userRepositoryMock.FindByEmailOrUsername(email)
                    .Returns((User?)null);

                // Act
                async Task Act() => await _sut.GetAuthenticationTokenAsync(email, _faker.Internet.Password());

                // Assert
                var ex = await Assert.ThrowsAnyAsync<EntityNotFoundException>(Act);
                Assert.Equal(email, ex.Key);
                Assert.Contains(nameof(User.Email), ex.KeyName);
                Assert.Contains(nameof(User.Username), ex.KeyName);
            }

            [Fact]
            public async Task Should_ThrowUnauthorizedException_When_PasswordIsInvalid()
            {
                // Arrange
                var userFaker = new UserFaker();
                User user = userFaker.Generate();
                _userRepositoryMock.FindByEmailOrUsername(user.Email)
                    .Returns(user);
                _userRepositoryMock.CheckPasswordAsync(Arg.Any<string>(), Arg.Any<string>())
                    .Returns(false);
                // Act
                async Task Act() => await _sut.GetAuthenticationTokenAsync(user.Email, _faker.Internet.Password());
                // Assert
                await Assert.ThrowsAnyAsync<UnauthorizedException>(Act);
            }
        }
    }
}
