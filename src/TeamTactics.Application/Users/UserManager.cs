
using Microsoft.Extensions.Logging;
using System.Text;
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Application.Common.Models;
using TeamTactics.Domain.Users;
using TeamTactics.Infrastructure.Tokens;

namespace TeamTactics.Application.Users
{
    public sealed class UserManager
    {
        private readonly IUserRepository _userRepository;
        private readonly IHashingService _hashingService;
        private readonly PasswordValidator _passwordValidator;
        private readonly IAuthTokenProvider _tokenProvider;
        private readonly ILogger<UserManager> _logger;

        public UserManager(
            IUserRepository userRepository,
            IHashingService hashingService,
            PasswordValidator passwordValidator,
            IAuthTokenProvider tokenProvider,
            ILogger<UserManager> logger)
        {
            _userRepository = userRepository;
            _hashingService = hashingService;
            _passwordValidator = passwordValidator;
            _tokenProvider = tokenProvider;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new user with the given username, email and password.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ValidationException"></exception>
        public async Task CreateUserAsync(string username, string email, string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(username);
            ArgumentException.ThrowIfNullOrWhiteSpace(email);
            ArgumentException.ThrowIfNullOrWhiteSpace(password);
            var validationResult = _passwordValidator.Validate(password);
            validationResult.ThrowIfInvalid();
            
            var existingUser = await _userRepository.FindByEmail(email);
            if (existingUser is not null)
            {
                throw new ArgumentException("User already exists", nameof(email));
            }

            // Hash password
            var salt = _hashingService.GenerateSalt();
            var passwordHash = _hashingService.Hash(Encoding.UTF8.GetBytes(password), salt);
            string passwordHashString = Convert.ToBase64String(passwordHash);

            User user = new User(
                username,
                email,
                new SecurityInfo(Convert.ToBase64String(salt)));
            
            int userId = await _userRepository.InsertAsync(user, passwordHashString);
            _logger.LogInformation("User '{username}' created with id {userId}", username, userId);
        }

        /// <summary>
        /// Gets an authentication token for the user with the given email and password.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UnauthorizedException"></exception>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task<AuthenticationToken> GetAuthenticationTokenAsync(string email, string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(email);
            ArgumentException.ThrowIfNullOrWhiteSpace(password);

            var user = await _userRepository.FindByEmail(email);
            if (user is null)
            {
                throw EntityNotFoundException.ForEntity<User>(email, nameof(User.Email));
            }

            var passwordHash = _hashingService.Hash(
                Encoding.UTF8.GetBytes(password),
                Encoding.UTF8.GetBytes(user.SecurityInfo.Salt));

            if (!await _userRepository.CheckPasswordAsync(Convert.ToBase64String(passwordHash)))
            {
                _logger.LogInformation("User with Id '{userId}' failed to login", user.Id);
                throw new UnauthorizedException("Invalid password");
            }

            return await _tokenProvider.GenerateTokenAsync(user);
        }
        public async Task<ProfileDto> GetProfileAsync(int id)
        {
            ProfileDto profile = await _userRepository.GetProfileAsync(id);
            if(profile is not null)
            {
                return profile;
            }
            //TODO: EntityNotFound exception
            throw new ArgumentException("No profile information exist");
        }
    }
}
