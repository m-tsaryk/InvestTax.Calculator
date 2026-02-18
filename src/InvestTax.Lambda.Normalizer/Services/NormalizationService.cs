using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using InvestTax.Core.Enums;
using InvestTax.Lambda.Normalizer.Models;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace InvestTax.Lambda.Normalizer.Services;

/// <summary>
/// Service for normalizing transaction data from CSV to structured JSON
/// </summary>
public class NormalizationService
{
    private readonly ILogger<NormalizationService> _logger;
    private readonly DateTimeZone _warsawTimeZone;

    public NormalizationService(ILogger<NormalizationService> logger)
    {
        _logger = logger;
        _warsawTimeZone = DateTimeZoneProviders.Tzdb["Europe/Warsaw"];
    }

    /// <summary>
    /// Normalizes CSV transaction data and writes structured JSON output
    /// </summary>
    /// <param name="inputPath">Path to input CSV file</param>
    /// <param name="outputPath">Path where normalized JSON will be written</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Normalization result with metadata</returns>
    public async Task<NormalizationResult> NormalizeAsync(
        string inputPath,
        string outputPath,
        CancellationToken cancellationToken)
    {
        var result = new NormalizationResult();
        var transactions = new List<NormalizedTransaction>();

        try
        {
            // Read and parse CSV with pipe delimiter
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "|",
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null
            };

            using (var reader = new StreamReader(inputPath))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<dynamic>().ToList();
                int id = 1;

                foreach (var record in records)
                {
                    var normalized = NormalizeRow(record, id++);
                    transactions.Add(normalized);
                }
            }

            // Sort by date (earliest first) to ensure correct FIFO order
            transactions = transactions.OrderBy(t => t.TransactionDate).ToList();

            // Group by ISIN
            var grouped = transactions.GroupBy(t => t.ISIN);

            foreach (var group in grouped)
            {
                result.TransactionGroups[group.Key] = new TransactionGroup
                {
                    ISIN = group.Key,
                    Ticker = group.First().Ticker,
                    Transactions = group.OrderBy(t => t.TransactionDate).ToList()
                };
            }

            result.TotalTransactions = transactions.Count;

            // Write JSON output with indentation for readability
            var json = JsonSerializer.Serialize(result.TransactionGroups, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync(outputPath, json, cancellationToken);

            _logger.LogInformation(
                "Normalized {Count} transactions into {Groups} ISIN groups",
                result.TotalTransactions,
                result.TransactionGroups.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error normalizing data from {InputPath}", inputPath);
            throw;
        }
    }

    /// <summary>
    /// Normalizes a single CSV row into a NormalizedTransaction
    /// </summary>
    private NormalizedTransaction NormalizeRow(dynamic record, int id)
    {
        var transaction = new NormalizedTransaction
        {
            Id = id,
            Action = ParseAction((string)record.Action),
            TransactionDate = ParseDate((string)record.Time),
            ISIN = ((string)record.ISIN).Trim().ToUpper(),
            Ticker = ((string)record.Ticker).Trim().ToUpper(),
            Name = ((string)record.Name).Trim(),
            Shares = ParseDecimal((string)record.NoOfShares),
            PricePerShare = ParseDecimal((string)record.PricePerShare),
            Currency = ((string)record.CurrencySymbol).Trim().ToUpper(),
            ExchangeRate = ParseDecimal((string)record.ExchangeRate),
            Total = ParseDecimal((string)record.Total),
            Notes = record.Notes?.ToString()?.Trim() ?? string.Empty
        };

        return transaction;
    }

    /// <summary>
    /// Parses transaction action string to enum
    /// </summary>
    private TransactionAction ParseAction(string action)
    {
        return action.Trim().ToLower() switch
        {
            "market buy" => TransactionAction.Buy,
            "market sell" => TransactionAction.Sell,
            "buy" => TransactionAction.Buy,
            "sell" => TransactionAction.Sell,
            _ => throw new InvalidOperationException($"Unknown action: {action}")
        };
    }

    /// <summary>
    /// Parses date string and converts to Europe/Warsaw timezone
    /// </summary>
    private DateTime ParseDate(string dateStr)
    {
        // Parse the date string
        var parsedDate = DateTime.Parse(dateStr, CultureInfo.InvariantCulture);

        // If the date is already specified as UTC, convert it
        if (parsedDate.Kind == DateTimeKind.Utc)
        {
            var instant = Instant.FromDateTimeUtc(parsedDate);
            var zonedDateTime = instant.InZone(_warsawTimeZone);
            return zonedDateTime.ToDateTimeUnspecified();
        }

        // Otherwise, treat it as Warsaw time
        var localDateTime = LocalDateTime.FromDateTime(parsedDate);
        var zonedLocal = _warsawTimeZone.AtLeniently(localDateTime);
        return zonedLocal.ToDateTimeUnspecified();
    }

    /// <summary>
    /// Parses decimal value with culture-invariant parsing
    /// </summary>
    private decimal ParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0m;
        }

        // Remove any thousand separators and normalize
        value = value.Replace(",", "").Replace(" ", "").Trim();
        return decimal.Parse(value, CultureInfo.InvariantCulture);
    }
}
