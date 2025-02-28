
namespace TeamTactics.Application.Common.Interfaces
{
    public interface IValidator<T>
    {
        public ValidationResult Validate(T data);
    }
}
