
using Microsoft.Extensions.Options;
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Application.Common.Options;

namespace TeamTactics.Application.Users
{
    public class PasswordValidator : IValidator<string>
    {
        private readonly PasswordSecurityOptions _passwordSecurityOptions;

        public PasswordValidator(IOptions<PasswordSecurityOptions> passwordSecurityOptions)
        {
            _passwordSecurityOptions = passwordSecurityOptions.Value;
        }

        public ValidationResult Validate(string password)
        {
            ValidationResult validationResult = new ValidationResult([]);
            if (password.Length < _passwordSecurityOptions.MinimumPasswordLength)
            {
                validationResult = validationResult
                    .WithError(nameof(password), $"Password must be a minimum of {_passwordSecurityOptions.MinimumPasswordLength} characters");
            }

            if (_passwordSecurityOptions.RequireLowercase)
            {
                if (!password.Any(char.IsLower))
                {
                    validationResult = validationResult
                        .WithError(nameof(password), "Password must contain a lowercase character");
                }
            }

            if (_passwordSecurityOptions.RequireUppercase)
            {
                if (!password.Any(char.IsUpper))
                {
                    validationResult = validationResult
                        .WithError(nameof(password), "Password must contain an uppercase character");
                }
            }

            if (_passwordSecurityOptions.RequireDigit)
            {
                if (!password.Any(char.IsDigit))
                {
                    validationResult = validationResult
                        .WithError(nameof(password), "Password must contain a digit");
                }
            }

            if (_passwordSecurityOptions.RequireAlphanumeric)
            {
                if (!password.Any(_passwordSecurityOptions.AlphanumricalCharacters.Contains))
                {
                    validationResult = validationResult
                        .WithError(nameof(password), "Password must contain an alphanumeric character");
                }
            }

            return validationResult;
        }
    }
}
