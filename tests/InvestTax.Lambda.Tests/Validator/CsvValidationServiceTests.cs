using InvestTax.Lambda.Validator.Models;
using InvestTax.Lambda.Validator.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace InvestTax.Lambda.Tests.Validator;

public class CsvValidationServiceTests
{
    private readonly Mock<ILogger<CsvValidationService>> _loggerMock;
    private readonly CsvValidationService _service;
    private readonly string _testDataPath;

    public CsvValidationServiceTests()
    {
        _loggerMock = new Mock<ILogger<CsvValidationService>>();
        _service = new CsvValidationService(_loggerMock.Object);
        _testDataPath = Path.Combine(Path.GetTempPath(), "InvestTaxTests");
        Directory.CreateDirectory(_testDataPath);
    }

    [Fact]
    public async Task ValidateFileAsync_ValidCsv_ReturnsSuccess()
    {
        // Arrange
        var testFile = Path.Combine(_testDataPath, $"valid_{Guid.NewGuid()}.csv");
        var csvContent = @"Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency (Price / share)|Exchange rate|Result|Total|Notes
Market buy|2024-01-15T10:30:00|US0378331005|AAPL|Apple Inc.|10|150.50|USD|3.75|USD 1505.00|PLN 5643.75|Test note
Market sell|2024-02-20T14:45:00|US5949181045|MSFT|Microsoft Corp.|5|380.25|USD|3.80|USD 1901.25|PLN 7224.75|";
        await File.WriteAllTextAsync(testFile, csvContent);

        // Act
        var result = await _service.ValidateFileAsync(testFile, CancellationToken.None);

        // Assert
        Assert.True(result.Valid);
        Assert.Equal(2, result.RowCount);
        Assert.Equal(2024, result.Year);
        Assert.Contains("USD", result.Currencies);
        Assert.Empty(result.Errors);

        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    public async Task ValidateFileAsync_InvalidAction_ReturnsErrors()
    {
        // Arrange
        var testFile = Path.Combine(_testDataPath, $"invalid_action_{Guid.NewGuid()}.csv");
        var csvContent = @"Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency (Price / share)|Exchange rate|Result|Total|Notes
Invalid Action|2024-01-15T10:30:00|US0378331005|AAPL|Apple Inc.|10|150.50|USD|3.75|USD 1505.00|PLN 5643.75|";
        await File.WriteAllTextAsync(testFile, csvContent);

        // Act
        var result = await _service.ValidateFileAsync(testFile, CancellationToken.None);

        // Assert
        Assert.False(result.Valid);
        Assert.Single(result.Errors);
        Assert.Contains(result.Errors, e => e.Column == "Action");

        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    public async Task ValidateFileAsync_InvalidISIN_ReturnsErrors()
    {
        // Arrange
        var testFile = Path.Combine(_testDataPath, $"invalid_isin_{Guid.NewGuid()}.csv");
        var csvContent = @"Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency (Price / share)|Exchange rate|Result|Total|Notes
Market buy|2024-01-15T10:30:00|INVALID|AAPL|Apple Inc.|10|150.50|USD|3.75|USD 1505.00|PLN 5643.75|";
        await File.WriteAllTextAsync(testFile, csvContent);

        // Act
        var result = await _service.ValidateFileAsync(testFile, CancellationToken.None);

        // Assert
        Assert.False(result.Valid);
        Assert.Contains(result.Errors, e => e.Column == "ISIN");

        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    public async Task ValidateFileAsync_EmptyFile_ReturnsError()
    {
        // Arrange
        var testFile = Path.Combine(_testDataPath, $"empty_{Guid.NewGuid()}.csv");
        var csvContent = @"Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency (Price / share)|Exchange rate|Result|Total|Notes";
        await File.WriteAllTextAsync(testFile, csvContent);

        // Act
        var result = await _service.ValidateFileAsync(testFile, CancellationToken.None);

        // Assert
        Assert.False(result.Valid);
        Assert.Contains(result.Errors, e => e.Message.Contains("no data rows"));

        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    public async Task ValidateFileAsync_MultipleCurrencies_CollectsAll()
    {
        // Arrange
        var testFile = Path.Combine(_testDataPath, $"multi_currency_{Guid.NewGuid()}.csv");
        var csvContent = @"Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency (Price / share)|Exchange rate|Result|Total|Notes
Market buy|2024-01-15T10:30:00|US0378331005|AAPL|Apple Inc.|10|150.50|USD|3.75|USD 1505.00|PLN 5643.75|
Market buy|2024-02-15T10:30:00|IE00B4BNMY34|ACWI|iShares MSCI ACWI|20|85.25|EUR|4.30|EUR 1705.00|PLN 7331.50|";
        await File.WriteAllTextAsync(testFile, csvContent);

        // Act
        var result = await _service.ValidateFileAsync(testFile, CancellationToken.None);

        // Assert
        Assert.True(result.Valid);
        Assert.Equal(2, result.Currencies.Count);
        Assert.Contains("USD", result.Currencies);
        Assert.Contains("EUR", result.Currencies);

        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    public async Task ValidateFileAsync_InvalidDateTime_ReturnsError()
    {
        // Arrange
        var testFile = Path.Combine(_testDataPath, $"invalid_date_{Guid.NewGuid()}.csv");
        var csvContent = @"Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency (Price / share)|Exchange rate|Result|Total|Notes
Market buy|invalid-date|US0378331005|AAPL|Apple Inc.|10|150.50|USD|3.75|USD 1505.00|PLN 5643.75|";
        await File.WriteAllTextAsync(testFile, csvContent);

        // Act
        var result = await _service.ValidateFileAsync(testFile, CancellationToken.None);

        // Assert
        Assert.False(result.Valid);
        Assert.Contains(result.Errors, e => e.Column == "Time");

        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    public async Task ValidateFileAsync_NegativeShares_ReturnsError()
    {
        // Arrange
        var testFile = Path.Combine(_testDataPath, $"negative_shares_{Guid.NewGuid()}.csv");
        var csvContent = @"Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency (Price / share)|Exchange rate|Result|Total|Notes
Market buy|2024-01-15T10:30:00|US0378331005|AAPL|Apple Inc.|-10|150.50|USD|3.75|USD 1505.00|PLN 5643.75|";
        await File.WriteAllTextAsync(testFile, csvContent);

        // Act
        var result = await _service.ValidateFileAsync(testFile, CancellationToken.None);

        // Assert
        Assert.False(result.Valid);
        Assert.Contains(result.Errors, e => e.Column == "NoOfShares");

        // Cleanup
        File.Delete(testFile);
    }
}
