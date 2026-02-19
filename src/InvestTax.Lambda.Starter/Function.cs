using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Extensions.NETCore.Setup;
using InvestTax.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace InvestTax.Lambda.Starter;

/// <summary>
/// Lambda function that is triggered by S3 ObjectCreated events and starts the Step Functions workflow
/// </summary>
public class Function
{
    private readonly IAmazonStepFunctions _stepFunctionsClient;
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly ILogger<Function> _logger;
    private readonly string _stateMachineArn;
    private readonly string _jobsTableName;
    private readonly string _processingBucketName;
    
    public Function()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();
        
        _stepFunctionsClient = serviceProvider.GetRequiredService<IAmazonStepFunctions>();
        _dynamoDbClient = serviceProvider.GetRequiredService<IAmazonDynamoDB>();
        _logger = serviceProvider.GetRequiredService<ILogger<Function>>();
        
        _stateMachineArn = Environment.GetEnvironmentVariable("STATE_MACHINE_ARN") 
            ?? throw new InvalidOperationException("STATE_MACHINE_ARN environment variable is not set");
        _jobsTableName = Environment.GetEnvironmentVariable("JOBS_TABLE") 
            ?? throw new InvalidOperationException("JOBS_TABLE environment variable is not set");
        _processingBucketName = Environment.GetEnvironmentVariable("PROCESSING_BUCKET") 
            ?? throw new InvalidOperationException("PROCESSING_BUCKET environment variable is not set");
    }
    
    private void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
        });
        
        services.AddAWSService<IAmazonStepFunctions>();
        services.AddAWSService<IAmazonDynamoDB>();
    }
    
    /// <summary>
    /// Lambda function handler for S3 event notifications
    /// Triggered when a CSV file is uploaded to the upload bucket
    /// </summary>
    /// <param name="s3Event">S3 event containing bucket and object information</param>
    /// <param name="context">Lambda context</param>
    public async Task FunctionHandler(S3Event s3Event, ILambdaContext context)
    {
        foreach (var record in s3Event.Records)
        {
            try
            {
                var bucketName = record.S3.Bucket.Name;
                var objectKey = record.S3.Object.Key;
                
                _logger.LogInformation("Processing S3 event for bucket {Bucket}, key {Key}", bucketName, objectKey);
                
                // Extract email from S3 object key
                // Expected format: email@example.com/filename.csv or user-identifier/filename.csv
                var parts = objectKey.Split('/', 2);
                if (parts.Length != 2)
                {
                    _logger.LogWarning("Invalid S3 key format: {Key}. Expected format: email@example.com/filename.csv", objectKey);
                    continue;
                }
                
                var email = parts[0];
                var filename = parts[1];
                
                // Validate email format (basic validation)
                if (!IsValidEmail(email))
                {
                    _logger.LogWarning("Invalid email format in S3 key: {Email}", email);
                    continue;
                }
                
                // Generate unique job ID
                var jobId = Guid.NewGuid().ToString();
                
                _logger.LogInformation("Creating job {JobId} for email {Email}, file {Filename}", jobId, email, filename);
                
                // Create job record in DynamoDB
                await CreateJobRecordAsync(jobId, email, objectKey, bucketName);
                
                // Start Step Functions execution
                await StartStateMachineExecutionAsync(jobId, email, objectKey, bucketName);
                
                _logger.LogInformation("Successfully started workflow for job {JobId}", jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing S3 event for bucket {Bucket}, key {Key}", 
                    record.S3.Bucket.Name, record.S3.Object.Key);
                throw;
            }
        }
    }
    
    private async Task CreateJobRecordAsync(string jobId, string email, string s3Key, string uploadBucket)
    {
        var now = DateTime.UtcNow;
        
        var putItemRequest = new PutItemRequest
        {
            TableName = _jobsTableName,
            Item = new Dictionary<string, AttributeValue>
            {
                ["JobId"] = new AttributeValue { S = jobId },
                ["Email"] = new AttributeValue { S = email },
                ["S3Key"] = new AttributeValue { S = s3Key },
                ["UploadBucket"] = new AttributeValue { S = uploadBucket },
                ["Status"] = new AttributeValue { S = JobStatus.Created.ToString() },
                ["CreatedAt"] = new AttributeValue { S = now.ToString("O") },
                ["UpdatedAt"] = new AttributeValue { S = now.ToString("O") }
            }
        };
        
        await _dynamoDbClient.PutItemAsync(putItemRequest);
        
        _logger.LogInformation("Created job record in DynamoDB for job {JobId}", jobId);
    }
    
    private async Task StartStateMachineExecutionAsync(string jobId, string email, string s3Key, string uploadBucket)
    {
        var executionInput = new
        {
            JobId = jobId,
            Email = email,
            S3Key = s3Key,
            UploadBucket = uploadBucket,
            ProcessingBucket = _processingBucketName
        };
        
        var startExecutionRequest = new StartExecutionRequest
        {
            StateMachineArn = _stateMachineArn,
            Name = jobId,
            Input = JsonSerializer.Serialize(executionInput, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            })
        };
        
        var response = await _stepFunctionsClient.StartExecutionAsync(startExecutionRequest);
        
        _logger.LogInformation("Started Step Functions execution {ExecutionArn} for job {JobId}", 
            response.ExecutionArn, jobId);
    }
    
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
