using InvestTax.Core.Enums;
using InvestTax.Core.Exceptions;
using InvestTax.Core.Interfaces;
using InvestTax.Core.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Net;
using System.Text.Json;

namespace InvestTax.Infrastructure.Services;

public class NBPApiClient : INBPApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NBPApiClient> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public NBPApiClient(HttpClient httpClient, ILogger<NBPApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Configure Polly retry policy for transient failures
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => 
                r.StatusCode == HttpStatusCode.RequestTimeout ||
                r.StatusCode == HttpStatusCode.TooManyRequests ||
                r.StatusCode >= HttpStatusCode.InternalServerError)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "NBP API call failed. Retry {RetryCount} after {Delay}s. Status: {StatusCode}",
                        retryCount,
                        timespan.TotalSeconds,
                        outcome.Result?.StatusCode);
                });
    }

    public async Task<decimal> GetExchangeRateAsync(
        Currency currency,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        if (currency == Currency.PLN)
        {
            return 1.0m; // PLN to PLN is always 1
        }

        var dateString = date.ToString("yyyy-MM-dd");
        var url = $"https://api.nbp.pl/api/exchangerates/rates/a/{currency.ToString().ToLower()}/{dateString}/?format=json";

        _logger.LogInformation("Fetching NBP exchange rate for {Currency} on {Date}", currency, dateString);

        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                return await _httpClient.GetAsync(url, cancellationToken);
            });

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // Date might be a weekend or holiday, try to find the most recent rate
                _logger.LogWarning("No NBP rate found for {Date}, searching for previous rate", dateString);
                return await GetMostRecentRateAsync(currency, date, cancellationToken);
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var rateResponse = JsonSerializer.Deserialize<NBPExchangeRateResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (rateResponse?.Rates == null || rateResponse.Rates.Count == 0)
            {
                throw new NBPApiException($"No exchange rate data returned for {currency} on {dateString}");
            }

            var rate = rateResponse.Rates[0].Mid;
            _logger.LogInformation("Retrieved NBP rate for {Currency} on {Date}: {Rate}", currency, dateString, rate);
            return rate;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch NBP exchange rate for {Currency} on {Date}", currency, dateString);
            throw new NBPApiException($"Failed to fetch exchange rate for {currency} on {dateString}", ex)
            {
                Currency = currency.ToString(),
                RequestedDate = date
            };
        }
    }

    public async Task<Dictionary<DateOnly, decimal>> GetExchangeRatesAsync(
        Currency currency,
        List<DateOnly> dates,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<DateOnly, decimal>();

        foreach (var date in dates)
        {
            var rate = await GetExchangeRateAsync(currency, date, cancellationToken);
            result[date] = rate;
        }

        return result;
    }

    private async Task<decimal> GetMostRecentRateAsync(
        Currency currency,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        // Try up to 7 days back to find a rate (covers weekends and short holidays)
        for (int i = 1; i <= 7; i++)
        {
            var previousDate = date.AddDays(-i);
            var dateString = previousDate.ToString("yyyy-MM-dd");
            var url = $"https://api.nbp.pl/api/exchangerates/rates/a/{currency.ToString().ToLower()}/{dateString}/?format=json";

            try
            {
                var response = await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _httpClient.GetAsync(url, cancellationToken);
                });

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var rateResponse = JsonSerializer.Deserialize<NBPExchangeRateResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (rateResponse?.Rates != null && rateResponse.Rates.Count > 0)
                    {
                        var rate = rateResponse.Rates[0].Mid;
                        _logger.LogInformation(
                            "Using NBP rate from {PreviousDate} for requested date {RequestedDate}: {Rate}",
                            dateString,
                            date.ToString("yyyy-MM-dd"),
                            rate);
                        return rate;
                    }
                }
            }
            catch (HttpRequestException)
            {
                // Continue to next date
                continue;
            }
        }

        throw new NBPApiException($"Could not find NBP exchange rate for {currency} within 7 days before {date}")
        {
            Currency = currency.ToString(),
            RequestedDate = date
        };
    }
}
