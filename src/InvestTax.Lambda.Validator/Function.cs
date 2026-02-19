using Amazon.Lambda.Core;
using Amazon.S3;
using InvestTax.Core.Interfaces;
using InvestTax.Core.Models;
using InvestTax.Lambda.Validator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace InvestTax.Lambda.Validator;

public class Function
{
    private readonly IS3Service _s3Service;
    private readonly CsvValidationService _validationService;
    private readonly ILogger<Function> _logger;
    
    public Function()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();
        
        _s3Service = serviceProvider.GetRequiredService<IS3Service>();
        _validationService = serviceProvider.GetRequiredService<CsvValidationService>();
        _logger = serviceProvider.GetRequiredService<ILogger<Function>>();
    }
    
    private void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
        });
        
        services.AddAWSService<IAmazonS3>();
        services.AddSingleton<IS3Service, InvestTax.Infrastructure.AWS.S3Service>();
        services.AddSingleton<CsvValidationService>();
    }
    
    /// <summary>
    /// Lambda function handler for CSV validation
    /// </summary>
    /// <param name="input">Input from Step Functions</param>
    /// <param name="context">Lambda context</param>
    /// <returns>Updated input object with validation results</returns>
    public async Task<LambdaInput> FunctionHandler(
        LambdaInput input, 
        ILambdaContext context)
    {
        _logger.LogInformation(
            "Starting validation for JobId: {JobId}, FileKey: {FileKey}",
            input.JobId, input.FileKey);
        
        var cancellationToken = CancellationToken.None;
        
        try
        {
            var uploadBucket = Environment.GetEnvironmentVariable("UPLOAD_BUCKET") 
                ?? throw new InvalidOperationException("UPLOAD_BUCKET environment variable not set");
            var processingBucket = Environment.GetEnvironmentVariable("PROCESSING_BUCKET") 
                ?? throw new InvalidOperationException("PROCESSING_BUCKET environment variable not set");
            
            // Use FileKey if set, otherwise fall back to S3Key for backward compatibility
            var fileKey = !string.IsNullOrEmpty(input.FileKey) ? input.FileKey : input.S3Key;
            
            if (string.IsNullOrEmpty(fileKey))
            {
                throw new ArgumentException("FileKey or S3Key must be provided");
            }
            
            // Download file from S3 to local temp directory
            var localPath = Path.Combine(Path.GetTempPath(), $"{input.JobId}.csv");
            _logger.LogInformation("Downloading file from s3://{Bucket}/{Key} to {LocalPath}",
                uploadBucket, fileKey, localPath);
            
            using (var stream = await _s3Service.GetObjectStreamAsync(
                uploadBucket, 
                fileKey, 
                cancellationToken))
            {
                using var fileStream = File.Create(localPath);
                await stream.CopyToAsync(fileStream, cancellationToken);
            }
            
            // Validate file
            _logger.LogInformation("Validating CSV file");
            var validationResult = await _validationService.ValidateFileAsync(
                localPath, 
                cancellationToken);
            
            if (validationResult.Valid)
            {
                // Upload validated file to processing bucket
                var validatedKey = $"validated/{input.JobId}.csv";
                _logger.LogInformation("Validation successful, uploading to s3://{Bucket}/{Key}",
                    processingBucket, validatedKey);
                
                using var uploadStream = File.OpenRead(localPath);
                await _s3Service.PutObjectAsync(
                    processingBucket, 
                    validatedKey, 
                    uploadStream, 
                    cancellationToken);
                
                input.ValidatedFileKey = validatedKey;
                input.RowCount = validationResult.RowCount;
                input.Year = validationResult.Year;
                input.Currencies = validationResult.Currencies;
                input.Stage = "VALIDATED";
                
                _logger.LogInformation(
                    "Validation successful: RowCount={RowCount}, Year={Year}, Currencies={Currencies}",
                    validationResult.RowCount, validationResult.Year, string.Join(",", validationResult.Currencies));
            }
            else
            {
                input.Stage = "VALIDATION_FAILED";
                input.ErrorMessage = JsonSerializer.Serialize(validationResult.Errors, new JsonSerializerOptions
                {
                    WriteIndented = false
                });
                
                _logger.LogWarning(
                    "Validation failed with {ErrorCount} errors: {Errors}",
                    validationResult.Errors.Count,
                    JsonSerializer.Serialize(validationResult.Errors.Take(5)));
            }
            
            // Cleanup local file
            if (File.Exists(localPath))
            {
                File.Delete(localPath);
                _logger.LogDebug("Cleaned up temporary file: {LocalPath}", localPath);
            }
            
            return input;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in validator function for JobId: {JobId}", input.JobId);
            input.Stage = "VALIDATION_ERROR";
            input.ErrorMessage = ex.Message;
            throw;
        }
    }
}
