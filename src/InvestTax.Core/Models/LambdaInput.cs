namespace InvestTax.Core.Models;

/// <summary>
/// Common input structure for Step Functions Lambda invocations
/// </summary>
public class LambdaInput
{
    public string JobId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string S3Key { get; set; } = string.Empty;
    public string UploadBucket { get; set; } = string.Empty;
    public string ProcessingBucket { get; set; } = string.Empty;
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
