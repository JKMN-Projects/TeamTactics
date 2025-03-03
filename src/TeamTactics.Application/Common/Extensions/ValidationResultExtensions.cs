namespace TeamTactics.Application.Common.Extensions
{
    internal static class ValidationResultExtensions
    {
        public static void ValidateOrThrow(this ValidationResult validationResult)
        {
            if (validationResult.IsValid)
            {
                return;
            }

            var ex = new ValidationException();
            
            foreach (var error in validationResult.Errors)
            {
                ex.AddError(error.PropertyName, error.ErrorMessage);
            }

            throw ex;
        }
    }
}
