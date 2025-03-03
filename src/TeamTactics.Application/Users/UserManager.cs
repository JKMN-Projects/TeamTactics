
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Application.Common.Options;
using TeamTactics.Domain.Users;

namespace TeamTactics.Application.Users
{
    public sealed class UserManager
    {
        private readonly IUserRepository _userRepository;
        private readonly IHashingService _hashingService;
        private readonly PasswordValidator _passwordValidator;
        private readonly ILogger<UserManager> _logger;

        public UserManager(
            IUserRepository userRepository,
            IHashingService hashingService,
            PasswordValidator passwordValidator,
            ILogger<UserManager> logger)
        {
            _userRepository = userRepository;
            _hashingService = hashingService;
            _passwordValidator = passwordValidator;
            _logger = logger;
        }

        public async Task CreateUserAsync(string userName, string email, string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userName);
            ArgumentException.ThrowIfNullOrWhiteSpace(email);
            ArgumentException.ThrowIfNullOrWhiteSpace(password);
            var validationResult = _passwordValidator.Validate(password);
            validationResult.ValidateOrThrow();
            
            var existingUser = await _userRepository.FindByEmail(email);
            if (existingUser is not null)
            {
                throw new ArgumentException("User already exists", nameof(email));
            }

            User user = new User(userName, email);

            // Hash password
            var salt = _hashingService.GenerateSalt();
            var passwordHash = _hashingService.Hash(Encoding.UTF8.GetBytes(password), salt);
            string passwordHashString = Convert.ToBase64String(passwordHash);
            
            int userId = await _userRepository.InsertAsync(user, passwordHashString);
            _logger.LogInformation("User '{userName}' created with id {userId}", userName, userId);
        }
    }
}
