namespace InvestTax.Lambda.Calculator.Models;

/// <summary>
/// Input for the Tax Calculator Lambda function
/// </summary>
public class CalculatorInput
{
    /// <summary>Job ID for tracking</summary>
    public string JobId { get; set; } = string.Empty;
    
    /// <summary>User email address</summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>Tax year being calculated</summary>
    public int Year { get; set; }
    
    /// <summary>S3 bucket containing processing files</summary>
    public string ProcessingBucket { get; set; } = string.Empty;
    
    /// <summary>S3 key for normalized transaction data (JSON)</summary>
    public string NormalizedFileKey { get; set; } = string.Empty;
    
    /// <summary>S3 key for NBP exchange rate map (JSON)</summary>
    public string RateMapKey { get; set; } = string.Empty;
}

/// <summary>
/// Output from the Tax Calculator Lambda function
/// </summary>
public class CalculatorOutput
{
    /// <summary>Job ID for tracking</summary>
    public string JobId { get; set; } = string.Empty;
    
    /// <summary>Processing status</summary>
    public bool Success { get; set; }
    
    /// <summary>S3 key for calculation result (JSON)</summary>
    public string CalculationResultKey { get; set; } = string.Empty;
    
    /// <summary>Error message if calculation failed</summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Exchange rate map for currency conversions
/// </summary>
public class RateMap
{
    /// <summary>Dictionary mapping "CURRENCY_YYYY-MM-DD" to PLN exchange rate</summary>
    public Dictionary<string, decimal> Rates { get; set; } = new();
}
