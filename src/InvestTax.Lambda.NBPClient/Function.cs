using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.S3;
using InvestTax.Core.Interfaces;
using InvestTax.Core.Models;
using InvestTax.Infrastructure.AWS;
using InvestTax.Infrastructure.Services;
using InvestTax.Lambda.NBPClient.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace InvestTax.Lambda.NBPClient;

/// <summary>
/// Lambda function to fetch PLN exchange rates from NBP API
/// </summary>
public class Function
{
    private readonly IS3Service _s3Service;
    private readonly RateFetcherService _rateFetcherService;
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
        _rateFetcherService = serviceProvider.GetRequiredService<RateFetcherService>();
        _logger = serviceProvider.GetRequiredService<ILogger<Function>>();
    }

    /// <summary>
    /// Constructor for dependency injection (testing)
    /// </summary>
    /// <param name="s3Service">S3 service instance</param>
    /// <param name="rateFetcherService">Rate fetcher service instance</param>
    /// <param name="logger">Logger instance</param>
    public Function(IS3Service s3Service, RateFetcherService rateFetcherService, ILogger<Function> logger)
    {
        _s3Service = s3Service;
        _rateFetcherService = rateFetcherService;
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

        // Configure HttpClient with Polly for NBPApiClient
        services.AddHttpClient<INBPApiClient, NBPApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.nbp.pl/");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        services.AddAWSService<IAmazonS3>();
        services.AddSingleton<IS3Service, S3Service>();
        services.AddSingleton<RateFetcherService>();
    }

    /// <summary>
    /// Lambda function handler to fetch exchange rates from NBP API
    /// </summary>
    /// <param name="input">Lambda input containing job metadata and S3 keys</param>
    /// <param name="context">Lambda execution context</param>
    /// <returns>Updated Lambda input with rate map key</returns>
    public async Task<LambdaInput> FunctionHandler(
        LambdaInput input,
        ILambdaContext context)
    {
        _logger.LogInformation(
            "Starting rate fetching for JobId: {JobId}, NormalizedFile: {NormalizedFileKey}",
            input.JobId,
            input.NormalizedFileKey);

        try
        {
            var processingBucket = Environment.GetEnvironmentVariable("PROCESSING_BUCKET")
                ?? throw new InvalidOperationException("PROCESSING_BUCKET environment variable not set");

            // Create temp file paths
            var inputPath = Path.Combine(Path.GetTempPath(), $"{input.JobId}_normalized.json");
            var outputPath = Path.Combine(Path.GetTempPath(), $"{input.JobId}_rates.json");

            try
            {
                // Download normalized JSON file from S3
                _logger.LogInformation("Downloading normalized file from s3://{Bucket}/{Key}",
                    processingBucket, input.NormalizedFileKey);

                await _s3Service.DownloadFileAsync(
                    processingBucket,
                    input.NormalizedFileKey,
                    inputPath,
                    CancellationToken.None);

                // Parse normalized data
                var json = await File.ReadAllTextAsync(inputPath, CancellationToken.None);
                var normalizedData = JsonSerializer.Deserialize<Dictionary<string, TransactionGroup>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }) ?? throw new InvalidOperationException("Failed to deserialize normalized data");

                _logger.LogInformation("Parsed normalized data with {Groups} transaction groups",
                    normalizedData.Count);

                // Fetch exchange rates
                _logger.LogInformation("Fetching exchange rates from NBP API");

                var result = await _rateFetcherService.FetchRatesAsync(
                    normalizedData,
                    outputPath,
                    CancellationToken.None);

                // Upload rate map JSON file to S3
                var rateMapKey = $"rates/{input.JobId}.json";

                _logger.LogInformation("Uploading rate map to s3://{Bucket}/{Key}",
                    processingBucket, rateMapKey);

                await _s3Service.UploadFileAsync(
                    processingBucket,
                    rateMapKey,
                    outputPath,
                    CancellationToken.None);

                // Update input with results
                input.RateMapKey = rateMapKey;
                input.Stage = "RATES_FETCHED";

                _logger.LogInformation(
                    "Rate fetching complete: {TotalRates} rates fetched for currencies: {Currencies}",
                    result.TotalRatesFetched,
                    string.Join(", ", result.Currencies));

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
            _logger.LogError(ex, "Error in NBP rate fetcher function for JobId: {JobId}", input.JobId);
            input.Stage = "RATE_FETCH_ERROR";
            input.ErrorMessage = ex.Message;
            throw;
        }
    }
}

