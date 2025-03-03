
namespace TeamTactics.Domain.Common.Exceptions
{
    [Serializable]
    public class DomainException : Exception
    {
        public string Code { get; }
        public string? Description { get; }

        public DomainException(string code, string? description) : base($"A domain rule was broken: '{code}'.")
        {
            Code = code;
            Description = description;
        }

        public DomainException(string code) : this(code, null)
        {
            
        }
    }
}
