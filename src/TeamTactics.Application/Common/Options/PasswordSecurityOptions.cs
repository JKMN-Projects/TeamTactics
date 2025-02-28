
using System.ComponentModel.DataAnnotations;

namespace TeamTactics.Application.Common.Options
{
    public sealed class PasswordSecurityOptions
    {
        [Range(8, 32)]
        public int MinimumPasswordLength { get; init; } = 8;

        public bool RequireUppercase { get; init; } = true;
        public bool RequireLowercase { get; init; } = true;
        public bool RequireDigit { get; init; } = true;
        public string AlphanumricalCharacters { get; init; } = "!*()";
        public bool RequireAlphanumeric { get; init; } = false;
    }
}
