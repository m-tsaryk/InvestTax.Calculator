using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using InvestTax.Core.Enums;
using InvestTax.Core.Interfaces;
using InvestTax.Core.Models;
using InvestTax.Lambda.Calculator.Models;
using InvestTax.Lambda.Calculator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace InvestTax.Lambda.Calculator;

/// <summary>
/// Lambda function to calculate capital gains tax using FIFO methodology
/// </summary>
public class Function
{
    private readonly IAmazonS3 _s3Client;
    private readonly ITaxCalculator _taxCalculator;
    private readonly ILogger<Function> _logger;

    /// <summary>
    /// Default constructor used by Lambda runtime
    /// </summary>
    public Function() : this(
        new AmazonS3Client(),
        CreateServiceProvider().GetRequiredService<ITaxCalculator>(),
        CreateServiceProvider().GetRequiredService<ILogger<Function>>())
    {
    }

    /// <summary>
    /// Constructor for dependency injection (testing)
    /// </summary>
    public Function(IAmazonS3 s3Client, ITaxCalculator taxCalculator, ILogger<Function> logger)
    {
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        _taxCalculator = taxCalculator ?? throw new ArgumentNullException(nameof(taxCalculator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lambda function handler for tax calculation
    /// </summary>
    /// <param name="input">Calculator input with S3 keys for normalized data and rate map</param>
    /// <param name="context">Lambda context</param>
    /// <returns>Calculator output with S3 key for calculation results</returns>
    public async Task<CalculatorOutput> FunctionHandler(CalculatorInput input, ILambdaContext context)
    {
        ArgumentNullException.ThrowIfNull(input);
        
        _logger.LogInformation("Starting tax calculation for Job ID: {JobId}, Year: {Year}", 
            input.JobId, input.Year);

        try
        {
            // Step 1: Load normalized transaction data from S3
            _logger.LogInformation("Loading normalized transactions from s3://{Bucket}/{Key}",
                input.ProcessingBucket, input.NormalizedFileKey);

            var normalizedData = await LoadNormalizedTransactionsAsync(
                input.ProcessingBucket, input.NormalizedFileKey, context.RemainingTime);

            // Step 2: Load exchange rate map from S3
            _logger.LogInformation("Loading exchange rates from s3://{Bucket}/{Key}",
                input.ProcessingBucket, input.RateMapKey);

            var rateMap = await LoadRateMapAsync(
                input.ProcessingBucket, input.RateMapKey, context.RemainingTime);

            // Step 3: Convert normalized transactions to Transaction objects with PLN prices
            var transactions = ConvertToTransactionsWithRates(normalizedData, rateMap);

            _logger.LogInformation("Converted {Count} normalized transactions to Transaction objects with PLN rates",
                transactions.Count);

            // Step 4: Calculate taxes using FIFO
            var taxSummary = _taxCalculator.CalculateTaxes(transactions, input.Year);

            _logger.LogInformation("Tax calculation complete: {TotalTransactions} transactions, " +
                "Gains: {Gains:N2} PLN, Losses: {Losses:N2} PLN, Net: {Net:N2} PLN, Tax: {Tax:N2} PLN",
                taxSummary.TotalTransactions, taxSummary.TotalGainsPLN, taxSummary.TotalLossesPLN,
                taxSummary.NetTaxableAmountPLN, taxSummary.EstimatedTaxPLN);

            // Step 5: Save calculation results to S3
            var resultKey = $"calculations/{input.JobId}.json";
            await SaveCalculationResultAsync(input.ProcessingBucket, resultKey, taxSummary, context.RemainingTime);

            _logger.LogInformation("Saved calculation results to s3://{Bucket}/{Key}",
                input.ProcessingBucket, resultKey);

            return new CalculatorOutput
            {
                JobId = input.JobId,
                Success = true,
                CalculationResultKey = resultKey
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tax calculation failed for Job ID: {JobId}", input.JobId);
            
            return new CalculatorOutput
            {
                JobId = input.JobId,
                Success = false,
                ErrorMessage = $"Tax calculation failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Load normalized transaction data from S3
    /// </summary>
    private async Task<Dictionary<string, TransactionGroup>> LoadNormalizedTransactionsAsync(
        string bucket, string key, TimeSpan remainingTime)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = bucket,
            Key = key
        };

        using var response = await _s3Client.GetObjectAsync(getRequest);
        using var reader = new StreamReader(response.ResponseStream);
        var json = await reader.ReadToEndAsync();

        var data = JsonSerializer.Deserialize<Dictionary<string, TransactionGroup>>(json);
        
        if (data is null)
        {
            throw new InvalidOperationException("Failed to deserialize normalized transaction data");
        }

        return data;
    }

    /// <summary>
    /// Load exchange rate map from S3
    /// </summary>
    private async Task<RateMap> LoadRateMapAsync(string bucket, string key, TimeSpan remainingTime)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = bucket,
            Key = key
        };

        using var response = await _s3Client.GetObjectAsync(getRequest);
        using var reader = new StreamReader(response.ResponseStream);
        var json = await reader.ReadToEndAsync();

        var rateMap = JsonSerializer.Deserialize<RateMap>(json);
        
        if (rateMap is null || rateMap.Rates is null)
        {
            throw new InvalidOperationException("Failed to deserialize rate map");
        }

        _logger.LogInformation("Loaded {Count} exchange rates", rateMap.Rates.Count);
        return rateMap;
    }

    /// <summary>
    /// Convert normalized transactions to Transaction objects with PLN prices applied
    /// </summary>
    private List<Transaction> ConvertToTransactionsWithRates(
        Dictionary<string, TransactionGroup> normalizedData,
        RateMap rateMap)
    {
        var transactions = new List<Transaction>();

        foreach (var group in normalizedData.Values)
        {
            foreach (var normalized in group.Transactions)
            {
                // Get exchange rate for this transaction
                var rateKey = $"{normalized.Currency}_{normalized.TransactionDate:yyyy-MM-dd}";
                
                if (!rateMap.Rates.TryGetValue(rateKey, out var exchangeRate))
                {
                    throw new InvalidOperationException(
                        $"Exchange rate not found for {rateKey}. " +
                        "Ensure NBP rate fetcher completed successfully.");
                }

                // Create Transaction with original currency but apply rate for PLN conversion
                var transaction = new Transaction
                {
                    Action = normalized.Action,
                    Time = normalized.TransactionDate,
                    ISIN = normalized.ISIN,
                    Ticker = normalized.Ticker,
                    Name = normalized.Name,
                    TransactionId = normalized.Id.ToString(),
                    Shares = normalized.Shares,
                    PricePerShare = normalized.PricePerShare,
                    PriceCurrency = ParseCurrency(normalized.Currency),
                    BrokerExchangeRate = exchangeRate,
                    Result = normalized.Total,
                    ResultCurrency = ParseCurrency(normalized.Currency),
                    Total = normalized.Total,
                    TotalCurrency = ParseCurrency(normalized.Currency)
                };

                transactions.Add(transaction);

                _logger.LogDebug("Transaction {Id}: {Action} {Shares} shares at {Price} {Currency}, " +
                    "Rate: {Rate:N4} PLN, PLN Price: {PlnPrice:N2}",
                    transaction.TransactionId, transaction.Action, transaction.Shares,
                    transaction.PricePerShare, transaction.PriceCurrency,
                    exchangeRate, transaction.PricePerShare * exchangeRate);
            }
        }

        return transactions.OrderBy(t => t.Time).ToList();
    }

    /// <summary>
    /// Save calculation results to S3
    /// </summary>
    private async Task SaveCalculationResultAsync(
        string bucket, string key, TaxSummary taxSummary, TimeSpan remainingTime)
    {
        var json = JsonSerializer.Serialize(taxSummary, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var putRequest = new PutObjectRequest
        {
            BucketName = bucket,
            Key = key,
            ContentBody = json,
            ContentType = "application/json"
        };

        await _s3Client.PutObjectAsync(putRequest);
    }

    /// <summary>
    /// Parse currency string to Currency enum
    /// </summary>
    private Currency ParseCurrency(string currencyCode)
    {
        return currencyCode.ToUpperInvariant() switch
        {
            "USD" => Currency.USD,
            "EUR" => Currency.EUR,
            "GBP" => Currency.GBP,
            "CHF" => Currency.CHF,
            "PLN" => Currency.PLN,
            _ => throw new ArgumentException($"Unsupported currency: {currencyCode}", nameof(currencyCode))
        };
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
        
        // Add services
        services.AddSingleton<ITaxCalculator, FifoTaxCalculator>();
        
        return services.BuildServiceProvider();
    }
}

