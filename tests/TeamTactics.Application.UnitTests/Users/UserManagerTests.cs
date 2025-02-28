
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Application.Common.Options;
using TeamTactics.Application.Users;
using TeamTactics.Domain.Users;

namespace TeamTactics.Application.UnitTests.Users
{
    public class UserManagerTests : TestBase
    {
        private readonly IUserRepository _userRepositoryMock;
        private readonly IHashingService _hashingServiceMock;
        private readonly PasswordValidator _passwordValidatorMock;
        private readonly ILogger<UserManager> _logger;
        private readonly UserManager _sut;

        public UserManagerTests()
        {
            _userRepositoryMock = Substitute.For<IUserRepository>();
            _hashingServiceMock = Substitute.For<IHashingService>();
            PasswordSecurityOptions passwordSecurityOptions = new PasswordSecurityOptions();
            IOptions<PasswordSecurityOptions> passwordSecurityOptionsMock = Options.Create(passwordSecurityOptions);
            _passwordValidatorMock = Substitute.For<PasswordValidator>(passwordSecurityOptionsMock);
            _logger = Substitute.For<ILogger<UserManager>>();

            _sut = new UserManager(
                _userRepositoryMock,
                _hashingServiceMock,
                _passwordValidatorMock,
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
                string password = faker.Internet.Password();
                Byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                _hashingServiceMock.GenerateSalt().Returns(faker.Random.Bytes(32));
                _hashingServiceMock.Hash(Arg.Any<byte[]>(), Arg.Any<byte[]>()).Returns(faker.Random.Bytes(32));

                // Act
                await _sut.CreateUserAsync(username, email, password);

                // Assert
                await _userRepositoryMock.Received(1)
                    .InsertAsync(Arg.Any<User>(), Arg.Is<string>(x => x != password));
            }
        }
    }
}
