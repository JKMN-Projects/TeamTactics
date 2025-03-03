namespace TeamTactics.Application.Common.Interfaces
{
    public interface IHashingService
    {
        public byte[] GenerateSalt();
        public byte[] Hash(byte[] toBeHashed, byte[] salt);
    }
}
