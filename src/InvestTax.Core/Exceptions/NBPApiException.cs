namespace InvestTax.Core.Exceptions;

/// <summary>
/// Exception thrown when NBP API calls fail
/// </summary>
public class NBPApiException : Exception
{
    public string? Currency { get; set; }
    public DateOnly? RequestedDate { get; set; }
    
    public NBPApiException(string message) : base(message)
    {
    }
    
    public NBPApiException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
