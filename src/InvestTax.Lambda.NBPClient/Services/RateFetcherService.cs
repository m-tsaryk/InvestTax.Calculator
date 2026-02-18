using System.Text.Json;
using InvestTax.Core.Enums;
using InvestTax.Core.Interfaces;
using InvestTax.Core.Models;
using InvestTax.Lambda.NBPClient.Models;
using Microsoft.Extensions.Logging;

namespace InvestTax.Lambda.NBPClient.Services;

/// <summary>
/// Service for fetching exchange rates from NBP API for all transactions
/// </summary>
public class RateFetcherService
{
    private readonly INBPApiClient _nbpApiClient;
    private readonly ILogger<RateFetcherService> _logger;

    public RateFetcherService(INBPApiClient nbpApiClient, ILogger<RateFetcherService> logger)
    {
        _nbpApiClient = nbpApiClient;
        _logger = logger;
    }

    /// <summary>
    /// Fetches exchange rates for all unique currency-date pairs in the normalized transactions
    /// </summary>
    /// <param name="normalizedData">Dictionary of transaction groups by ISIN</param>
    /// <param name="outputPath">Path where rate map JSON will be written</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing metadata about fetched rates</returns>
    public async Task<RateFetchResult> FetchRatesAsync(
        Dictionary<string, TransactionGroup> normalizedData,
        string outputPath,
        CancellationToken cancellationToken)
    {
        try
        {
            // Extract unique currency-date pairs
            var rateRequests = ExtractUniqueCurrencyDatePairs(normalizedData);
            _logger.LogInformation("Found {Count} unique currency-date pairs requiring exchange rates", rateRequests.Count);

            // Fetch rates for each unique pair
            var rateMap = new RateMap();
            var currenciesProcessed = new HashSet<string>();

            foreach (var request in rateRequests)
            {
                // Parse currency enum
                if (!Enum.TryParse<Currency>(request.Currency, ignoreCase: true, out var currencyEnum))
                {
                    _logger.LogWarning("Unknown currency {Currency}, skipping", request.Currency);
                    continue;
                }

                // PLN doesn't need to be fetched from NBP
                if (currencyEnum == Currency.PLN)
                {
                    var key = GetRateKey(request.Currency, request.Date);
                    rateMap.Rates[key] = 1.0m;
                    currenciesProcessed.Add(request.Currency);
                    continue;
                }

                try
                {
                    var rate = await _nbpApiClient.GetExchangeRateAsync(
                        currencyEnum,
                        request.Date,
                        cancellationToken);

                    var key = GetRateKey(request.Currency, request.Date);
                    rateMap.Rates[key] = rate;
                    currenciesProcessed.Add(request.Currency);

                    _logger.LogDebug("Fetched rate for {Currency} on {Date}: {Rate} PLN", 
                        request.Currency, request.Date, rate);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, 
                        "Failed to fetch rate for {Currency} on {Date}", 
                        request.Currency, request.Date);
                    throw;
                }
            }

            // Write rate map to JSON file
            var json = JsonSerializer.Serialize(rateMap, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync(outputPath, json, cancellationToken);

            _logger.LogInformation(
                "Fetched {Count} exchange rates for {CurrencyCount} currencies",
                rateMap.Rates.Count,
                currenciesProcessed.Count);

            return new RateFetchResult
            {
                TotalRatesFetched = rateMap.Rates.Count,
                Currencies = currenciesProcessed.OrderBy(c => c).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching exchange rates");
            throw;
        }
    }

    /// <summary>
    /// Extracts unique currency-date pairs from normalized transaction data
    /// </summary>
    private List<RateRequest> ExtractUniqueCurrencyDatePairs(
        Dictionary<string, TransactionGroup> normalizedData)
    {
        var uniquePairs = new HashSet<(string Currency, DateOnly Date)>();

        foreach (var group in normalizedData.Values)
        {
            foreach (var transaction in group.Transactions)
            {
                // Skip if currency is empty or already PLN
                if (string.IsNullOrWhiteSpace(transaction.Currency))
                {
                    continue;
                }

                var currency = transaction.Currency.ToUpper();
                var date = DateOnly.FromDateTime(transaction.TransactionDate);

                uniquePairs.Add((currency, date));
            }
        }

        return uniquePairs
            .Select(pair => new RateRequest 
            { 
                Currency = pair.Currency, 
                Date = pair.Date 
            })
            .OrderBy(r => r.Currency)
            .ThenBy(r => r.Date)
            .ToList();
    }

    /// <summary>
    /// Generates a consistent key for storing rates in the map
    /// </summary>
    private string GetRateKey(string currency, DateOnly date)
    {
        return $"{currency.ToUpper()}_{date:yyyy-MM-dd}";
    }
}
