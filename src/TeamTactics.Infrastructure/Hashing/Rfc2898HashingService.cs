using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using TeamTactics.Application.Common.Interfaces;

namespace TeamTactics.Infrastructure.Hashing
{
    public class Rfc2898HashingService : IHashingService
    {
        private HashingOptions _options;

        public Rfc2898HashingService(
            IOptions<HashingOptions> options)
        {
            _options = options.Value;
        }
        public byte[] GenerateSalt()
        {
            var randomNumberGenerator = RandomNumberGenerator.Create();

            byte[] randomBytes = new byte[_options.ByteLength];
            randomNumberGenerator.GetBytes(randomBytes);
            return randomBytes;
        }

        public byte[] Hash(byte[] toBeHashed, byte[] salt)
        {
            if (toBeHashed.Length == 0) throw new ArgumentException("Hash byte was empty", nameof(toBeHashed));

            using (var rfc2898 = new Rfc2898DeriveBytes(toBeHashed, salt, _options.EncryptionRounds, HashAlgorithmName.SHA256))
            {
                return rfc2898.GetBytes(_options.ByteLength);
            };
        }
    }
}
