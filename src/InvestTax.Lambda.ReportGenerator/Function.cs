using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using InvestTax.Core.Models;
using InvestTax.Lambda.ReportGenerator.Models;
using InvestTax.Lambda.ReportGenerator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace InvestTax.Lambda.ReportGenerator;

/// <summary>
/// Lambda function to generate plain text tax calculation reports
/// </summary>
public class Function
{
    private readonly IAmazonS3 _s3Client;
    private readonly TextReportGenerator _reportGenerator;
    private readonly ILogger<Function> _logger;

    /// <summary>
    /// Default constructor used by Lambda runtime
    /// </summary>
    public Function() : this(
        new AmazonS3Client(),
        new TextReportGenerator(),
        CreateServiceProvider().GetRequiredService<ILogger<Function>>())
    {
    }

    /// <summary>
    /// Constructor for dependency injection (testing)
    /// </summary>
    public Function(IAmazonS3 s3Client, TextReportGenerator reportGenerator, ILogger<Function> logger)
    {
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        _reportGenerator = reportGenerator ?? throw new ArgumentNullException(nameof(reportGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lambda function handler for report generation
    /// </summary>
    /// <param name="input">Report input with S3 key for calculation results</param>
    /// <param name="context">Lambda context</param>
    /// <returns>Report output with S3 key for generated reports</returns>
    public async Task<ReportOutput> FunctionHandler(ReportInput input, ILambdaContext context)
    {
        ArgumentNullException.ThrowIfNull(input);
        
        _logger.LogInformation("Starting report generation for Job ID: {JobId}, Year: {Year}", 
            input.JobId, input.Year);

        try
        {
            // Step 1: Load tax calculation results from S3
            _logger.LogInformation("Loading calculation results from s3://{Bucket}/{Key}",
                input.ProcessingBucket, input.CalculationResultKey);

            var taxSummary = await LoadCalculationResultsAsync(
                input.ProcessingBucket, 
                input.CalculationResultKey, 
                context.RemainingTime);

            _logger.LogInformation("Loaded tax summary: {Transactions} transactions, " +
                "Gains: {Gains:N2} PLN, Losses: {Losses:N2} PLN, Tax: {Tax:N2} PLN",
                taxSummary.TotalTransactions, taxSummary.TotalGainsPLN, 
                taxSummary.TotalLossesPLN, taxSummary.EstimatedTaxPLN);

            // Step 2: Generate plain text report
            _logger.LogInformation("Generating plain text report");

            var textReport = _reportGenerator.GenerateReport(taxSummary, input.JobId, input.Email);

            _logger.LogInformation("Generated text report: {Length} characters", textReport.Length);

            // Step 3: Save text report to S3
            var textReportKey = $"reports/{input.JobId}.txt";
            await SaveTextReportAsync(input.ProcessingBucket, textReportKey, textReport, context.RemainingTime);

            _logger.LogInformation("Saved text report to s3://{Bucket}/{Key}",
                input.ProcessingBucket, textReportKey);

            return new ReportOutput
            {
                JobId = input.JobId,
                Success = true,
                TextReportKey = textReportKey
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report generation failed for Job ID: {JobId}", input.JobId);
            
            return new ReportOutput
            {
                JobId = input.JobId,
                Success = false,
                ErrorMessage = $"Report generation failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Load tax calculation results from S3
    /// </summary>
    private async Task<TaxSummary> LoadCalculationResultsAsync(
        string bucket, 
        string key, 
        TimeSpan remainingTime)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = bucket,
            Key = key
        };

        using var response = await _s3Client.GetObjectAsync(getRequest);
        using var reader = new StreamReader(response.ResponseStream);
        var json = await reader.ReadToEndAsync();

        var taxSummary = JsonSerializer.Deserialize<TaxSummary>(json);
        
        if (taxSummary is null)
        {
            throw new InvalidOperationException("Failed to deserialize tax calculation results");
        }

        return taxSummary;
    }

    /// <summary>
    /// Save plain text report to S3
    /// </summary>
    private async Task SaveTextReportAsync(
        string bucket, 
        string key, 
        string textReport, 
        TimeSpan remainingTime)
    {
        var bytes = Encoding.UTF8.GetBytes(textReport);

        using var memoryStream = new MemoryStream(bytes);
        
        var putRequest = new PutObjectRequest
        {
            BucketName = bucket,
            Key = key,
            InputStream = memoryStream,
            ContentType = "text/plain; charset=utf-8"
        };

        await _s3Client.PutObjectAsync(putRequest);
    }

    /// <summary>
    /// Create service provider for dependency injection
    /// </summary>
    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
        });
        
        return services.BuildServiceProvider();
    }
}

