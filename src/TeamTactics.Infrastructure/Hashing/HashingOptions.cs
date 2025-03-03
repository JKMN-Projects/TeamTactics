
using System.ComponentModel.DataAnnotations;

namespace TeamTactics.Infrastructure.Hashing
{
    public class HashingOptions
    {
        [Range(1000, int.MaxValue)]
        public int EncryptionRounds { get; init; } = 100_000;
        [Range(8, 256)]
        public int ByteLength { get; init; } = 32;
    }
}
