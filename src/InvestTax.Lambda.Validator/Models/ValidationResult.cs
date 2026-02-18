namespace InvestTax.Lambda.Validator.Models;

/// <summary>
/// Result of CSV file validation
/// </summary>
public class ValidationResult
{
    public bool Valid { get; set; }
    public int RowCount { get; set; }
    public int Year { get; set; }
    public List<string> Currencies { get; set; } = new();
    public string ValidatedFileKey { get; set; } = string.Empty;
    public List<ValidationError> Errors { get; set; } = new();
}

/// <summary>
/// Represents a single validation error
/// </summary>
public class ValidationError
{
    public int Row { get; set; }
    public string Column { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
