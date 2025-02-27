
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
        private readonly PasswordSecurityOptions _passwordSecurityOptions;
        private readonly ILogger<UserManager> _logger;

        public UserManager(
            IUserRepository userRepository,
            IHashingService hashingService,
            IOptions<PasswordSecurityOptions> passwordSecurityOptions,
            ILogger<UserManager> logger)
        {
            _userRepository = userRepository;
            _hashingService = hashingService;
            _passwordSecurityOptions = passwordSecurityOptions.Value;
            _logger = logger;
        }

        public async Task CreateUserAsync(string userName, string email, string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userName);
            ArgumentException.ThrowIfNullOrWhiteSpace(email);
            ArgumentException.ThrowIfNullOrWhiteSpace(password);
            if (!ValidatePassword(password))
            {
                throw new ArgumentException("Password does not meet security requirements", nameof(password));
            }
            
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

        private bool ValidatePassword(string password)
        {
            if(_passwordSecurityOptions.RequireLowercase)
            {
                if (!password.Any(char.IsLower))
                {
                    return false;
                }
            }

            if(_passwordSecurityOptions.RequireUppercase)
            {
                if (!password.Any(char.IsUpper))
                {
                    return false;
                }
            }

            if (_passwordSecurityOptions.RequireDigit)
            {
                if (!password.Any(char.IsDigit))
                {
                    return false;
                }
            }

            if (_passwordSecurityOptions.RequireAlphanumeric)
            {
                if (!password.Any(_passwordSecurityOptions.AlphanumricalCharacters.Contains))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
