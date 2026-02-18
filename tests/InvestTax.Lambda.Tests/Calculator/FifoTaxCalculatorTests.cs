using FluentAssertions;
using InvestTax.Core.Enums;
using InvestTax.Core.Models;
using InvestTax.Lambda.Calculator.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace InvestTax.Lambda.Tests.Calculator;

public class FifoTaxCalculatorTests
{
    private readonly Mock<ILogger<FifoTaxCalculator>> _loggerMock;
    private readonly FifoTaxCalculator _calculator;

    public FifoTaxCalculatorTests()
    {
        _loggerMock = new Mock<ILogger<FifoTaxCalculator>>();
        _calculator = new FifoTaxCalculator(_loggerMock.Object);
    }

    [Fact]
    public void CalculateTaxes_SimpleBuyAndSell_CalculatesCorrectGain()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                Action = TransactionAction.Buy,
                Time = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc),
                ISIN = "US0378331005",
                Ticker = "AAPL",
                Name = "Apple Inc.",
                TransactionId = "BUY-001",
                Shares = 10m,
                PricePerShare = 150m,
                PriceCurrency = Currency.USD,
                BrokerExchangeRate = 4.0m,
                Result = 1500m,
                ResultCurrency = Currency.USD,
                Total = 1500m,
                TotalCurrency = Currency.USD
            },
            new()
            {
                Action = TransactionAction.Sell,
                Time = new DateTime(2024, 6, 15, 10, 0, 0, DateTimeKind.Utc),
                ISIN = "US0378331005",
                Ticker = "AAPL",
                Name = "Apple Inc.",
                TransactionId = "SELL-001",
                Shares = 10m,
                PricePerShare = 180m,
                PriceCurrency = Currency.USD,
                BrokerExchangeRate = 4.1m,
                Result = 1800m,
                ResultCurrency = Currency.USD,
                Total = 1800m,
                TotalCurrency = Currency.USD
            }
        };

        var result = _calculator.CalculateTaxes(transactions, 2024);

        result.Should().NotBeNull();
        result.Year.Should().Be(2024);
        result.TotalTransactions.Should().Be(1);
        result.Calculations.Should().HaveCount(1);

        var calculation = result.Calculations[0];
        calculation.ISIN.Should().Be("US0378331005");
        calculation.CostBasisPLN.Should().Be(10m * 150m * 4.0m); // 6000 PLN
        calculation.ProceedsPLN.Should().Be(10m * 180m * 4.1m); // 7380 PLN
        calculation.GainLossPLN.Should().Be(1380m); // 7380 - 6000 = 1380 PLN
        calculation.IsGain.Should().BeTrue();

        result.TotalGainsPLN.Should().Be(1380m);
        result.TotalLossesPLN.Should().Be(0m);
        result.NetTaxableAmountPLN.Should().Be(1380m);
        result.EstimatedTaxPLN.Should().Be(1380m * 0.19m); // 262.20 PLN
    }

    [Fact]
    public void CalculateTaxes_SimpleBuyAndSellWithLoss_CalculatesCorrectLoss()
    {
        var transactions = new List<Transaction>
        {
            CreateTransaction(TransactionAction.Buy, "US0378331005", 100, 150m, Currency.USD, 4.0m, new DateTime(2024, 1, 1)),
            CreateTransaction(TransactionAction.Sell, "US0378331005", 100, 120m, Currency.USD, 4.0m, new DateTime(2024, 6, 1))
        };

        var result = _calculator.CalculateTaxes(transactions, 2024);

        result.TotalTransactions.Should().Be(1);
        result.Calculations[0].GainLossPLN.Should().Be(-12000m); // (120 - 150) * 100 * 4.0 = -12000
        result.Calculations[0].IsGain.Should().BeFalse();
        result.TotalGainsPLN.Should().Be(0m);
        result.TotalLossesPLN.Should().Be(12000m);
        result.NetTaxableAmountPLN.Should().Be(-12000m);
    }

    [Fact]
    public void CalculateTaxes_MultipleBuysAndSingleSell_UsesFifoOrdering()
    {
        var transactions = new List<Transaction>
        {
            CreateTransaction(TransactionAction.Buy, "US0378331005", 50, 100m, Currency.USD, 4.0m, new DateTime(2024, 1, 1)),
            CreateTransaction(TransactionAction.Buy, "US0378331005", 50, 120m, Currency.USD, 4.0m, new DateTime(2024, 2, 1)),
            CreateTransaction(TransactionAction.Sell, "US0378331005", 60, 150m, Currency.USD, 4.0m, new DateTime(2024, 6, 1))
        };

        var result = _calculator.CalculateTaxes(transactions, 2024);

        result.Calculations.Should().HaveCount(1);
        var calculation = result.Calculations[0];
        
        calculation.MatchedBuys.Should().HaveCount(2);
        calculation.MatchedBuys[0].SharesMatched.Should().Be(50m); // First 50 shares from first buy
        calculation.MatchedBuys[0].CostBasisPLN.Should().Be(50m * 100m * 4.0m); // 20000
        
        calculation.MatchedBuys[1].SharesMatched.Should().Be(10m); // Next 10 shares from second buy
        calculation.MatchedBuys[1].CostBasisPLN.Should().Be(10m * 120m * 4.0m); // 4800

        calculation.CostBasisPLN.Should().Be(24800m); // 20000 + 4800
        calculation.ProceedsPLN.Should().Be(60m * 150m * 4.0m); // 36000
        calculation.GainLossPLN.Should().Be(11200m); // 36000 - 24800
    }

    [Fact]
    public void CalculateTaxes_PartialFills_MatchesCorrectly()
    {
        var transactions = new List<Transaction>
        {
            CreateTransaction(TransactionAction.Buy, "US0378331005", 100, 100m, Currency.USD, 4.0m, new DateTime(2024, 1, 1)),
            CreateTransaction(TransactionAction.Sell, "US0378331005", 30, 110m, Currency.USD, 4.0m, new DateTime(2024, 3, 1)),
            CreateTransaction(TransactionAction.Sell, "US0378331005", 40, 115m, Currency.USD, 4.0m, new DateTime(2024, 6, 1)),
            CreateTransaction(TransactionAction.Sell, "US0378331005", 30, 120m, Currency.USD, 4.0m, new DateTime(2024, 9, 1))
        };

        var result = _calculator.CalculateTaxes(transactions, 2024);

        result.TotalTransactions.Should().Be(3);
        result.Calculations.Should().HaveCount(3);

        result.Calculations[0].SellTransaction.Shares.Should().Be(30m);
        result.Calculations[0].GainLossPLN.Should().Be(30m * 10m * 4.0m); // 1200
        
        result.Calculations[1].SellTransaction.Shares.Should().Be(40m);
        result.Calculations[1].GainLossPLN.Should().Be(40m * 15m * 4.0m); // 2400
        
        result.Calculations[2].SellTransaction.Shares.Should().Be(30m);
        result.Calculations[2].GainLossPLN.Should().Be(30m * 20m * 4.0m); // 2400

        result.TotalGainsPLN.Should().Be(6000m);
    }

    [Fact]
    public void CalculateTaxes_MultipleStocks_CalculatesSeparately()
    {
        var transactions = new List<Transaction>
        {
            CreateTransaction(TransactionAction.Buy, "US0378331005", 10, 100m, Currency.USD, 4.0m, new DateTime(2024, 1, 1)),
            CreateTransaction(TransactionAction.Buy, "US5949181045", 20, 200m, Currency.USD, 4.0m, new DateTime(2024, 1, 1)),
            CreateTransaction(TransactionAction.Sell, "US0378331005", 10, 120m, Currency.USD, 4.0m, new DateTime(2024, 6, 1)),
            CreateTransaction(TransactionAction.Sell, "US5949181045", 20, 180m, Currency.USD, 4.0m, new DateTime(2024, 6, 1))
        };

        var result = _calculator.CalculateTaxes(transactions, 2024);

        result.TotalTransactions.Should().Be(2);
        result.Calculations.Should().HaveCount(2);

        var appleCalc = result.Calculations.First(c => c.ISIN == "US0378331005");
        appleCalc.GainLossPLN.Should().Be(10m * 20m * 4.0m); // 800
        appleCalc.IsGain.Should().BeTrue();

        var msftCalc = result.Calculations.First(c => c.ISIN == "US5949181045");
        msftCalc.GainLossPLN.Should().Be(20m * -20m * 4.0m); // -1600
        msftCalc.IsGain.Should().BeFalse();

        result.TotalGainsPLN.Should().Be(800m);
        result.TotalLossesPLN.Should().Be(1600m);
        result.NetTaxableAmountPLN.Should().Be(-800m);
    }

    [Fact]
    public void CalculateTaxes_PLNCurrency_NoExchangeRateApplied()
    {
        var transactions = new List<Transaction>
        {
            CreateTransaction(TransactionAction.Buy, "PLPKO0000016", 100, 50m, Currency.PLN, 1.0m, new DateTime(2024, 1, 1)),
            CreateTransaction(TransactionAction.Sell, "PLPKO0000016", 100, 55m, Currency.PLN, 1.0m, new DateTime(2024, 6, 1))
        };

        var result = _calculator.CalculateTaxes(transactions, 2024);

        result.Calculations[0].CostBasisPLN.Should().Be(5000m); // 100 * 50
        result.Calculations[0].ProceedsPLN.Should().Be(5500m); // 100 * 55
        result.Calculations[0].GainLossPLN.Should().Be(500m);
    }

    [Fact]
    public void CalculateTaxes_SellWithoutBuy_ThrowsException()
    {
        var transactions = new List<Transaction>
        {
            CreateTransaction(TransactionAction.Sell, "US0378331005", 10, 150m, Currency.USD, 4.0m, new DateTime(2024, 1, 1))
        };

        var act = () => _calculator.CalculateTaxes(transactions, 2024);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*insufficient buy positions*");
    }

    [Fact]
    public void CalculateTaxes_SellMoreThanBought_ThrowsException()
    {
        var transactions = new List<Transaction>
        {
            CreateTransaction(TransactionAction.Buy, "US0378331005", 50, 100m, Currency.USD, 4.0m, new DateTime(2024, 1, 1)),
            CreateTransaction(TransactionAction.Sell, "US0378331005", 100, 150m, Currency.USD, 4.0m, new DateTime(2024, 6, 1))
        };

        var act = () => _calculator.CalculateTaxes(transactions, 2024);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*insufficient buy positions*");
    }

    [Fact]
    public void CalculateTaxes_UnsoldShares_AddsWarning()
    {
        var transactions = new List<Transaction>
        {
            CreateTransaction(TransactionAction.Buy, "US0378331005", 100, 100m, Currency.USD, 4.0m, new DateTime(2024, 1, 1)),
            CreateTransaction(TransactionAction.Sell, "US0378331005", 60, 120m, Currency.USD, 4.0m, new DateTime(2024, 6, 1))
        };

        var result = _calculator.CalculateTaxes(transactions, 2024);

        result.Warnings.Should().HaveCount(1);
        result.Warnings[0].Should().Contain("US0378331005");
        result.Warnings[0].Should().Contain("40");
        result.Warnings[0].Should().Contain("remain unsold");
    }

    [Fact]
    public void CalculateTaxes_MixedGainsAndLosses_CalculatesNetCorrectly()
    {
        var transactions = new List<Transaction>
        {
            CreateTransaction(TransactionAction.Buy, "STOCK1", 100, 100m, Currency.USD, 4.0m, new DateTime(2024, 1, 1)),
            CreateTransaction(TransactionAction.Buy, "STOCK2", 100, 200m, Currency.USD, 4.0m, new DateTime(2024, 1, 1)),
            CreateTransaction(TransactionAction.Sell, "STOCK1", 100, 150m, Currency.USD, 4.0m, new DateTime(2024, 6, 1)), // Gain
            CreateTransaction(TransactionAction.Sell, "STOCK2", 100, 180m, Currency.USD, 4.0m, new DateTime(2024, 6, 1))  // Loss
        };

        var result = _calculator.CalculateTaxes(transactions, 2024);

        result.TotalGainsPLN.Should().Be(20000m); // (150 - 100) * 100 * 4.0
        result.TotalLossesPLN.Should().Be(8000m); // (180 - 200) * 100 * 4.0 = -8000, abs = 8000
        result.NetTaxableAmountPLN.Should().Be(12000m); // 20000 - 8000
        result.EstimatedTaxPLN.Should().Be(2280m); // 12000 * 0.19
    }

    [Fact]
    public void CalculateTaxes_TransactionsOutOfOrder_SortsCorrectly()
    {
        var transactions = new List<Transaction>
        {
            CreateTransaction(TransactionAction.Sell, "US0378331005", 50, 150m, Currency.USD, 4.0m, new DateTime(2024, 6, 1)),
            CreateTransaction(TransactionAction.Buy, "US0378331005", 100, 100m, Currency.USD, 4.0m, new DateTime(2024, 1, 1)),
            CreateTransaction(TransactionAction.Buy, "US0378331005", 50, 120m, Currency.USD, 4.0m, new DateTime(2024, 2, 1))
        };

        var result = _calculator.CalculateTaxes(transactions, 2024);

        result.Calculations[0].MatchedBuys.Should().HaveCount(1);
        result.Calculations[0].MatchedBuys[0].BuyTransaction.Time.Should().Be(new DateTime(2024, 1, 1));
        result.Calculations[0].MatchedBuys[0].SharesMatched.Should().Be(50m);
    }

    [Fact]
    public void CalculateTaxes_NullTransactions_ThrowsArgumentNullException()
    {
        var act = () => _calculator.CalculateTaxes(null!, 2024);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("transactions");
    }

    [Fact]
    public void CalculateTaxes_EmptyTransactions_ReturnsEmptySummary()
    {
        var transactions = new List<Transaction>();

        var result = _calculator.CalculateTaxes(transactions, 2024);

        result.Should().NotBeNull();
        result.Year.Should().Be(2024);
        result.Calculations.Should().BeEmpty();
        result.TotalGainsPLN.Should().Be(0m);
        result.TotalLossesPLN.Should().Be(0m);
        result.NetTaxableAmountPLN.Should().Be(0m);
        result.EstimatedTaxPLN.Should().Be(0m);
        result.TotalTransactions.Should().Be(0);
    }

    [Fact]
    public void CalculateTaxes_DifferentExchangeRatesForBuyAndSell_UsesCorrectRates()
    {
        var transactions = new List<Transaction>
        {
            CreateTransaction(TransactionAction.Buy, "US0378331005", 10, 100m, Currency.USD, 4.0m, new DateTime(2024, 1, 1)),
            CreateTransaction(TransactionAction.Sell, "US0378331005", 10, 100m, Currency.USD, 4.5m, new DateTime(2024, 6, 1))
        };

        var result = _calculator.CalculateTaxes(transactions, 2024);

        result.Calculations[0].CostBasisPLN.Should().Be(4000m); // 10 * 100 * 4.0
        result.Calculations[0].ProceedsPLN.Should().Be(4500m); // 10 * 100 * 4.5
        result.Calculations[0].GainLossPLN.Should().Be(500m); // Exchange rate difference creates gain
    }

    [Fact]
    public void CalculateTaxes_RoundingToTwoDecimals_IsAppliedCorrectly()
    {
        var transactions = new List<Transaction>
        {
            CreateTransaction(TransactionAction.Buy, "US0378331005", 3m, 100.333m, Currency.USD, 4.123m, new DateTime(2024, 1, 1)),
            CreateTransaction(TransactionAction.Sell, "US0378331005", 3m, 150.667m, Currency.USD, 4.567m, new DateTime(2024, 6, 1))
        };

        var result = _calculator.CalculateTaxes(transactions, 2024);

        // Cost: 3 * 100.333 * 4.123 = 1241.021859 rounded to 1241.02
        // Proceeds: 3 * 150.667 * 4.567 = 2064.289881 rounded to 2064.29
        result.Calculations[0].CostBasisPLN.Should().BeApproximately(1241.02m, 0.01m);
        result.Calculations[0].ProceedsPLN.Should().BeApproximately(2064.29m, 0.01m);
        result.Calculations[0].GainLossPLN.Should().BeApproximately(823.27m, 0.01m);
    }

    private static Transaction CreateTransaction(
        TransactionAction action,
        string isin,
        decimal shares,
        decimal price,
        Currency currency,
        decimal exchangeRate,
        DateTime time)
    {
        return new Transaction
        {
            Action = action,
            Time = time,
            ISIN = isin,
            Ticker = "TEST",
            Name = "Test Stock",
            TransactionId = $"{action}-{Guid.NewGuid()}",
            Shares = shares,
            PricePerShare = price,
            PriceCurrency = currency,
            BrokerExchangeRate = exchangeRate,
            Result = shares * price,
            ResultCurrency = currency,
            Total = shares * price,
            TotalCurrency = currency
        };
    }
}
