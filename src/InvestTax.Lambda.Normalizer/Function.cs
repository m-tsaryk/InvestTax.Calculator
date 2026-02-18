using Amazon.Lambda.Core;
using Amazon.S3;
using InvestTax.Core.Interfaces;
using InvestTax.Core.Models;
using InvestTax.Infrastructure.AWS;
using InvestTax.Lambda.Normalizer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace InvestTax.Lambda.Normalizer;

/// <summary>
/// Lambda function to normalize transaction data from CSV to structured JSON
/// </summary>
public class Function
{
    private readonly IS3Service _s3Service;
    private readonly NormalizationService _normalizationService;
    private readonly ILogger<Function> _logger;

    /// <summary>
    /// Default constructor for Lambda runtime
    /// </summary>
    public Function()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        _s3Service = serviceProvider.GetRequiredService<IS3Service>();
        _normalizationService = serviceProvider.GetRequiredService<NormalizationService>();
        _logger = serviceProvider.GetRequiredService<ILogger<Function>>();
    }

    /// <summary>
    /// Constructor for dependency injection (testing)
    /// </summary>
    /// <param name="s3Service">S3 service instance</param>
    /// <param name="normalizationService">Normalization service instance</param>
    /// <param name="logger">Logger instance</param>
    public Function(IS3Service s3Service, NormalizationService normalizationService, ILogger<Function> logger)
    {
        _s3Service = s3Service;
        _normalizationService = normalizationService;
        _logger = logger;
    }

    /// <summary>
    /// Configures dependency injection services
    /// </summary>
    private void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddLambdaLogger();
        });

        services.AddAWSService<IAmazonS3>();
        services.AddSingleton<IS3Service, S3Service>();
        services.AddSingleton<NormalizationService>();
    }

    /// <summary>
    /// Lambda function handler to normalize transaction data
    /// </summary>
    /// <param name="input">Lambda input containing job metadata and S3 keys</param>
    /// <param name="context">Lambda execution context</param>
    /// <returns>Updated Lambda input with normalized file key</returns>
    public async Task<LambdaInput> FunctionHandler(
        LambdaInput input,
        ILambdaContext context)
    {
        _logger.LogInformation(
            "Starting normalization for JobId: {JobId}, ValidatedFile: {ValidatedFileKey}",
            input.JobId,
            input.ValidatedFileKey);

        try
        {
            var processingBucket = Environment.GetEnvironmentVariable("PROCESSING_BUCKET")
                ?? throw new InvalidOperationException("PROCESSING_BUCKET environment variable not set");

            // Create temp file paths
            var inputPath = Path.Combine(Path.GetTempPath(), $"{input.JobId}_input.csv");
            var outputPath = Path.Combine(Path.GetTempPath(), $"{input.JobId}_normalized.json");

            try
            {
                // Download validated CSV file from S3
                _logger.LogInformation("Downloading validated file from s3://{Bucket}/{Key}",
                    processingBucket, input.ValidatedFileKey);
                
                await _s3Service.DownloadFileAsync(
                    processingBucket,
                    input.ValidatedFileKey,
                    inputPath,
                    CancellationToken.None);

                // Normalize data
                _logger.LogInformation("Normalizing data from {InputPath}", inputPath);
                
                var result = await _normalizationService.NormalizeAsync(
                    inputPath,
                    outputPath,
                    CancellationToken.None);

                // Upload normalized JSON file to S3
                var normalizedKey = $"normalized/{input.JobId}.json";
                
                _logger.LogInformation("Uploading normalized file to s3://{Bucket}/{Key}",
                    processingBucket, normalizedKey);
                
                await _s3Service.UploadFileAsync(
                    processingBucket,
                    normalizedKey,
                    outputPath,
                    CancellationToken.None);

                // Update input with results
                input.NormalizedFileKey = normalizedKey;
                input.Stage = "NORMALIZED";

                _logger.LogInformation(
                    "Normalization complete: {TotalTransactions} transactions in {Groups} ISIN groups",
                    result.TotalTransactions,
                    result.TransactionGroups.Count);

                return input;
            }
            finally
            {
                // Cleanup temp files
                if (File.Exists(inputPath))
                {
                    File.Delete(inputPath);
                    _logger.LogDebug("Cleaned up temporary file: {InputPath}", inputPath);
                }
                
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                    _logger.LogDebug("Cleaned up temporary file: {OutputPath}", outputPath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in normalizer function for JobId: {JobId}", input.JobId);
            input.Stage = "NORMALIZATION_ERROR";
            input.ErrorMessage = ex.Message;
            throw;
        }
    }
}

