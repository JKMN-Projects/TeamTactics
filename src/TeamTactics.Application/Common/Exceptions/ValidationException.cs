
namespace TeamTactics.Application.Common.Exceptions
{
    public class ValidationException : Exception
    {
        private Dictionary<string, List<string>> _errors;
        public IReadOnlyDictionary<string, List<string>> Errors => _errors;


        public ValidationException() : base("One or more validation errors occured.")
        {
            _errors = new Dictionary<string, List<string>>();
        }

        public ValidationException(string paramName, IEnumerable<string> validationErrors) : this()
        {
            _errors.Add(paramName, validationErrors.ToList());
        }
        public ValidationException(string paramName, string validationError) : this(paramName, [validationError])
        {
            
        }
        public ValidationException(string validationError) : this(string.Empty, [validationError])
        {
            
        }


        public void AddError(string validationError)
        {
            AddError(string.Empty, validationError);
        }

        public void AddError(string paramName, string validationError)
        {
            if (_errors.ContainsKey(paramName))
            {
                _errors[paramName].Add(validationError);
            }
            else
            {
                _errors.Add(paramName, [validationError]);
            }
        }

        public void AddError(string paramName, IEnumerable<string> validationErrors)
        {
            if (_errors.ContainsKey(paramName))
            {
                _errors[paramName].AddRange(validationErrors);
            }
            else
            {
                _errors.Add(paramName, validationErrors.ToList());
            }
        }
    }
}
