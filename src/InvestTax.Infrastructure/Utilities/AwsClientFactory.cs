using Amazon;
using Amazon.Runtime;

namespace InvestTax.Infrastructure.Configuration;

/// <summary>
/// Helper class to create AWS client configurations for LocalStack or real AWS
/// </summary>
public static class AwsClientFactory
{
    /// <summary>
    /// Creates AWS credentials based on configuration
    /// </summary>
    public static AWSCredentials CreateCredentials(LocalAwsConfig config)
    {
        if (config.UseLocalStack)
        {
            return new BasicAWSCredentials(config.AccessKey, config.SecretKey);
        }
        
        // Use default credential chain for real AWS (IAM roles, environment variables, etc.)
        return FallbackCredentialsFactory.GetCredentials();
    }
    
    /// <summary>
    /// Gets the AWS region endpoint
    /// </summary>
    public static RegionEndpoint GetRegion(LocalAwsConfig config)
    {
        return RegionEndpoint.GetBySystemName(config.Region);
    }
    
    /// <summary>
    /// Gets the service URL (LocalStack or null for real AWS)
    /// </summary>
    public static string? GetServiceUrl(LocalAwsConfig config)
    {
        return config.UseLocalStack ? config.LocalStackEndpoint : null;
    }
}
