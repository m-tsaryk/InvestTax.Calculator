namespace InvestTax.Core.Exceptions;

/// <summary>
/// Exception thrown when CSV validation fails
/// </summary>
public class ValidationException : Exception
{
    public List<string> ValidationErrors { get; }
    
    public ValidationException(string message, List<string> errors) : base(message)
    {
        ValidationErrors = errors;
    }
    
    public ValidationException(string message) : this(message, new List<string>())
    {
    }
}
