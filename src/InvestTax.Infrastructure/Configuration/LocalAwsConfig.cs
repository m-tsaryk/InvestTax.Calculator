namespace InvestTax.Infrastructure.Configuration;

/// <summary>
/// Configuration for AWS services with LocalStack support
/// </summary>
public class LocalAwsConfig
{
    /// <summary>
    /// Whether to use LocalStack instead of real AWS services
    /// </summary>
    public bool UseLocalStack { get; set; } = false;
    
    /// <summary>
    /// LocalStack endpoint URL
    /// </summary>
    public string LocalStackEndpoint { get; set; } = "http://localhost:4566";
    
    /// <summary>
    /// AWS region
    /// </summary>
    public string Region { get; set; } = "eu-central-1";
    
    /// <summary>
    /// AWS access key (use 'test' for LocalStack)
    /// </summary>
    public string AccessKey { get; set; } = "test";
    
    /// <summary>
    /// AWS secret key (use 'test' for LocalStack)
    /// </summary>
    public string SecretKey { get; set; } = "test";
}
