using CsvHelper;
using CsvHelper.Configuration;
using InvestTax.Lambda.Validator.Models;
using InvestTax.Lambda.Validator.Validators;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace InvestTax.Lambda.Validator.Services;

/// <summary>
/// Service responsible for validating CSV file structure and content
/// </summary>
public class CsvValidationService
{
    private readonly CsvRowValidator _validator;
    private readonly ILogger<CsvValidationService> _logger;
    
    public CsvValidationService(ILogger<CsvValidationService> logger)
    {
        _validator = new CsvRowValidator();
        _logger = logger;
    }
    
    /// <summary>
    /// Validates a CSV file from local path
    /// </summary>
    /// <param name="filePath">Local file path to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with errors if any</returns>
    public async Task<ValidationResult> ValidateFileAsync(
        string filePath, 
        CancellationToken cancellationToken)
    {
        var result = new ValidationResult();
        var currencies = new HashSet<string>();
        int? year = null;
        
        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "|",
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null // Ignore missing fields
            };
            
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);
            
            var records = csv.GetRecords<CsvRow>().ToList();
            
            if (records.Count == 0)
            {
                result.Errors.Add(new ValidationError
                {
                    Row = 0,
                    Column = "File",
                    Message = "File contains no data rows"
                });
                return result;
            }
            
            if (records.Count > 100000)
            {
                result.Errors.Add(new ValidationError
                {
                    Row = 0,
                    Column = "File",
                    Message = "File exceeds maximum 100,000 rows"
                });
                return result;
            }
            
            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];
                var rowNumber = i + 2; // Account for header row
                
                var validationResult = _validator.Validate(record);
                
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        result.Errors.Add(new ValidationError
                        {
                            Row = rowNumber,
                            Column = error.PropertyName,
                            Message = error.ErrorMessage
                        });
                    }
                }
                
                // Extract year from first valid date
                if (year == null && DateTime.TryParse(record.Time, out var dt))
                {
                    year = dt.Year;
                }
                
                // Collect unique currencies
                if (!string.IsNullOrEmpty(record.CurrencySymbol))
                {
                    currencies.Add(record.CurrencySymbol.ToUpper());
                }
            }
            
            result.Valid = result.Errors.Count == 0;
            result.RowCount = records.Count;
            result.Year = year ?? DateTime.Now.Year;
            result.Currencies = currencies.OrderBy(c => c).ToList();
            
            _logger.LogInformation(
                "Validation complete: Valid={Valid}, Rows={RowCount}, Errors={ErrorCount}, Year={Year}, Currencies={Currencies}",
                result.Valid, result.RowCount, result.Errors.Count, result.Year, string.Join(",", result.Currencies));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating CSV file");
            result.Errors.Add(new ValidationError
            {
                Row = 0,
                Column = "File",
                Message = $"File parsing error: {ex.Message}"
            });
            return result;
        }
    }
}
