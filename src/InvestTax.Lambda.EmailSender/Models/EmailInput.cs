namespace InvestTax.Lambda.EmailSender.Models;

/// <summary>
/// Input for the Email Sender Lambda function
/// </summary>
public class EmailInput
{
    /// <summary>Job ID for tracking</summary>
    public string JobId { get; set; } = string.Empty;
    
    /// <summary>User email address (recipient)</summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>Tax year being reported</summary>
    public int Year { get; set; }
    
    /// <summary>S3 bucket containing processing files</summary>
    public string ProcessingBucket { get; set; } = string.Empty;
    
    /// <summary>S3 key for text report</summary>
    public string? TextReportKey { get; set; }
    
    /// <summary>Indicates if this is a success or error email</summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>Error message for failure emails</summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>Stage where error occurred (for error emails)</summary>
    public string? ErrorStage { get; set; }
}

/// <summary>
/// Output from the Email Sender Lambda function
/// </summary>
public class EmailOutput
{
    /// <summary>Job ID for tracking</summary>
    public string JobId { get; set; } = string.Empty;
    
    /// <summary>Processing status</summary>
    public bool Success { get; set; }
    
    /// <summary>SES Message ID</summary>
    public string? MessageId { get; set; }
    
    /// <summary>Error message if email sending failed</summary>
    public string? ErrorMessage { get; set; }
}
