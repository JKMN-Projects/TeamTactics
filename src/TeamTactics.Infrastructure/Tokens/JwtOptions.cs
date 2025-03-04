using System.ComponentModel.DataAnnotations;

namespace TeamTactics.Infrastructure.Tokens
{
    public sealed class JwtOptions
    {
        [Required]
        public required string Key { get; init; }

        [Required]
        public required int ValidityInMinutes { get; init; }

        [Required]
        public required string Url { get; init; }
    }
}
