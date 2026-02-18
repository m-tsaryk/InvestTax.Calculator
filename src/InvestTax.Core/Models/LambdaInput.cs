namespace InvestTax.Core.Models;

/// <summary>
/// Common input structure for Step Functions Lambda invocations
/// </summary>
public class LambdaInput
{
    public string JobId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FileKey { get; set; } = string.Empty;
    public string S3Key { get; set; } = string.Empty;
    public string UploadBucket { get; set; } = string.Empty;
    public string ProcessingBucket { get; set; } = string.Empty;
    
    // Validator output
    public string ValidatedFileKey { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public int Year { get; set; }
    public List<string> Currencies { get; set; } = new();
    
    // Pipeline stage tracking
    public string Stage { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    
    // Normalizer output
    public string NormalizedFileKey { get; set; } = string.Empty;
    
    // Calculator output
    public string CalculationResultKey { get; set; } = string.Empty;
    
    // Report Generator output
    public string ReportFileKey { get; set; } = string.Empty;
}

/// <summary>
/// Common output structure for Step Functions Lambda invocations
/// </summary>
public class LambdaOutput
{
    public bool Success { get; set; }
    public string JobId { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
