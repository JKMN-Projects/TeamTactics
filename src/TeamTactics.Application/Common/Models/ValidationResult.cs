namespace TeamTactics.Application.Common.Models
{
    public record ValidationResult
    {
        private readonly List<ValidationFailure> _errors = [];
        public IEnumerable<ValidationFailure> Errors => _errors;

        public bool IsValid => _errors.Count == 0;
        public ValidationResult(IEnumerable<ValidationFailure> errors)
        {
            _errors = errors.ToList();
        }

        public ValidationResult WithError(string propertyName, string errorMessage)
        {
            _errors.Add(new ValidationFailure(propertyName, errorMessage));
            return this;
        }
    }

    public record ValidationFailure(string PropertyName, string ErrorMessage);
}
