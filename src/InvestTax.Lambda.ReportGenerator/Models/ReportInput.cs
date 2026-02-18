namespace InvestTax.Lambda.ReportGenerator.Models;

/// <summary>
/// Input for the Report Generator Lambda function
/// </summary>
public class ReportInput
{
    /// <summary>Job ID for tracking</summary>
    public string JobId { get; set; } = string.Empty;
    
    /// <summary>User email address</summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>Tax year being reported</summary>
    public int Year { get; set; }
    
    /// <summary>S3 bucket containing processing files</summary>
    public string ProcessingBucket { get; set; } = string.Empty;
    
    /// <summary>S3 key for calculation result (JSON)</summary>
    public string CalculationResultKey { get; set; } = string.Empty;
}

/// <summary>
/// Output from the Report Generator Lambda function
/// </summary>
public class ReportOutput
{
    /// <summary>Job ID for tracking</summary>
    public string JobId { get; set; } = string.Empty;
    
    /// <summary>Processing status</summary>
    public bool Success { get; set; }
    
    /// <summary>S3 key for text report</summary>
    public string TextReportKey { get; set; } = string.Empty;
    
    /// <summary>Error message if report generation failed</summary>
    public string? ErrorMessage { get; set; }
}
