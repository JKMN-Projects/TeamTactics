
using Microsoft.Extensions.Logging;
using System.Text;
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Application.Common.Models;
using TeamTactics.Application.Tournaments;
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
            ITournamentRepository tournamentRepository,
            IHashingService hashingService,
            PasswordValidator passwordValidator,
            IAuthTokenProvider tokenProvider,
            ILogger<UserManager> logger)
        {
            _userRepository = userRepository;
            _tournamentRepository = tournamentRepository;
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

            bool emailTaken = await _userRepository.CheckIfEmailExistsAsync(email);
            bool usernameTaken = await _userRepository.CheckIfUsernameExistsAsync(username);
            if (emailTaken)
                throw new ArgumentException("Email already exists", nameof(email));
            if (usernameTaken)
                throw new ArgumentException("Username already taken", nameof(username));

            // Hash password
            var salt = _hashingService.GenerateSalt();
            string saltString = Convert.ToBase64String(salt);
            var passwordHash = _hashingService.Hash(Encoding.UTF8.GetBytes(password), salt);
            string passwordHashString = Convert.ToBase64String(passwordHash);

            User user = new User(
                username,
                email);
            
            User newUser = await _userRepository.InsertAsync(user, passwordHashString, saltString);
            _logger.LogInformation("User '{username}' created with id {userId}", newUser.Username, newUser.Id);
        }

        /// <summary>
        /// Gets an authentication token for the user with the given email and password.
        /// </summary>
        /// <param name="emailOrUsername"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UnauthorizedException"></exception>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task<AuthenticationToken> GetAuthenticationTokenAsync(string emailOrUsername, string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(emailOrUsername);
            ArgumentException.ThrowIfNullOrWhiteSpace(password);

            var user = await _userRepository.FindByEmailOrUsername(emailOrUsername);
            if (user is null)
            {
                throw EntityNotFoundException.ForEntity<User>(emailOrUsername, "UsernameAndEmail");
            }

            string? saltString = await _userRepository.GetUserSaltAsync(user.Id);
            if (saltString is null)
            {
                throw new InvalidOperationException("Unable to get user salt.");
            }

            var passwordHash = _hashingService.Hash(
                Encoding.UTF8.GetBytes(password),
                Convert.FromBase64String(saltString));

            if (!await _userRepository.CheckPasswordAsync(emailOrUsername, Convert.ToBase64String(passwordHash)))
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

            throw EntityNotFoundException.ForEntity<User>(id, nameof(User.Id));
        }
    }
}
