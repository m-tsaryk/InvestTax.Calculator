using InvestTax.Core.Enums;

namespace InvestTax.Core.Models;

/// <summary>
/// Job tracking entity stored in DynamoDB
/// </summary>
public class Job
{
    /// <summary>Unique job identifier (GUID)</summary>
    public string JobId { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>User email address</summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>S3 key of uploaded file</summary>
    public string S3Key { get; set; } = string.Empty;
    
    /// <summary>Current job status</summary>
    public JobStatus Status { get; set; } = JobStatus.Created;
    
    /// <summary>Job creation timestamp (ISO 8601)</summary>
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
    
    /// <summary>Last update timestamp (ISO 8601)</summary>
    public string UpdatedAt { get; set; } = DateTime.UtcNow.ToString("o");
    
    /// <summary>Job completion timestamp (ISO 8601)</summary>
    public string? CompletedAt { get; set; }
    
    /// <summary>Error message if status is Failed</summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>Step Functions execution ARN</summary>
    public string? ExecutionArn { get; set; }
    
    /// <summary>Number of transactions processed</summary>
    public int? TransactionCount { get; set; }
    
    /// <summary>Processing duration in seconds</summary>
    public double? DurationSeconds { get; set; }
}
