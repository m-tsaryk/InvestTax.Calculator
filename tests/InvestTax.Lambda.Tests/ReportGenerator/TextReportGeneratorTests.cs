using FluentAssertions;
using InvestTax.Core.Enums;
using InvestTax.Core.Models;
using InvestTax.Lambda.ReportGenerator.Services;

namespace InvestTax.Lambda.Tests.ReportGenerator;

public class TextReportGeneratorTests
{
    private readonly TextReportGenerator _generator;

    public TextReportGeneratorTests()
    {
        _generator = new TextReportGenerator();
    }

    [Fact]
    public void GenerateReport_WithValidSummary_ContainsAllSections()
    {
        var taxSummary = CreateSimpleTaxSummary();

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        report.Should().NotBeNullOrEmpty();
        report.Should().Contain("POLISH CAPITAL GAINS TAX CALCULATION");
        report.Should().Contain("SUMMARY");
        report.Should().Contain("DETAILED TRANSACTIONS");
        report.Should().Contain("CALCULATION METHODOLOGY");
        report.Should().Contain("DISCLAIMER");
        report.Should().Contain("END OF REPORT");
    }

    [Fact]
    public void GenerateReport_WithValidSummary_ContainsJobId()
    {
        var taxSummary = CreateSimpleTaxSummary();
        var jobId = "test-job-" + Guid.NewGuid();

        var report = _generator.GenerateReport(taxSummary, jobId, "test@example.com");

        report.Should().Contain($"Job ID: {jobId}");
    }

    [Fact]
    public void GenerateReport_WithValidSummary_ContainsYear()
    {
        var taxSummary = CreateSimpleTaxSummary();
        taxSummary.Year = 2024;

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        report.Should().Contain("Tax Year: 2024");
    }

    [Fact]
    public void GenerateReport_WithValidSummary_ContainsEmail()
    {
        var taxSummary = CreateSimpleTaxSummary();
        var email = "user@example.com";

        var report = _generator.GenerateReport(taxSummary, "test-job-123", email);

        report.Should().Contain($"Report generated for: {email}");
    }

    [Fact]
    public void GenerateReport_WithValidSummary_DisplaysSummaryValues()
    {
        var taxSummary = CreateSimpleTaxSummary();
        taxSummary.TotalGainsPLN = 10000m;
        taxSummary.TotalLossesPLN = 2000m;
        taxSummary.NetTaxableAmountPLN = 8000m;

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        report.Should().Contain("10,000.00 PLN");
        report.Should().Contain("2,000.00 PLN");
        report.Should().Contain("8,000.00 PLN");
        report.Should().Contain("1,520.00 PLN"); // 19% of 8000
    }

    [Fact]
    public void GenerateReport_WithTransactions_DisplaysISIN()
    {
        var taxSummary = CreateSimpleTaxSummary();
        var calculation = taxSummary.Calculations[0];
        calculation.ISIN = "US0378331005";

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        report.Should().Contain("ISIN: US0378331005");
    }

    [Fact]
    public void GenerateReport_WithTransactions_DisplaysTickerAndName()
    {
        var taxSummary = CreateSimpleTaxSummary();
        var calculation = taxSummary.Calculations[0];
        calculation.SellTransaction.Ticker = "AAPL";
        calculation.SellTransaction.Name = "Apple Inc.";

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        report.Should().Contain("Ticker: AAPL");
        report.Should().Contain("Name: Apple Inc.");
    }

    [Fact]
    public void GenerateReport_WithTransaction_DisplaysSellDetails()
    {
        var taxSummary = CreateSimpleTaxSummary();
        var calculation = taxSummary.Calculations[0];
        calculation.SellTransaction.Shares = 100m;
        calculation.SellTransaction.PricePerShare = 150.50m;
        calculation.SellTransaction.PriceCurrency = Currency.USD;
        calculation.ProceedsPLN = 15050m;

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        report.Should().Contain("Shares Sold:      100.000000");
        report.Should().Contain("Sell Price:       150.5000 USD");
        report.Should().Contain("Proceeds (PLN):   15,050.00");
    }

    [Fact]
    public void GenerateReport_WithMatchedBuys_DisplaysBuyDetails()
    {
        var taxSummary = CreateSimpleTaxSummary();
        var calculation = taxSummary.Calculations[0];
        
        var matchedBuy = new MatchedBuy
        {
            BuyTransaction = CreateTransaction(TransactionAction.Buy, 50m, 100m, new DateTime(2024, 1, 15)),
            SharesMatched = 50m,
            CostBasisPLN = 5000m
        };
        
        calculation.MatchedBuys.Add(matchedBuy);
        calculation.CostBasisPLN = 5000m;

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        report.Should().Contain("Matched Buys (FIFO):");
        report.Should().Contain("2024-01-15");
        report.Should().Contain("50.000000 shares");
        report.Should().Contain("Cost Basis (PLN): 5,000.00");
    }

    [Fact]
    public void GenerateReport_WithGain_DisplaysGainLabel()
    {
        var taxSummary = CreateSimpleTaxSummary();
        var calculation = taxSummary.Calculations[0];
        calculation.GainLossPLN = 5000m;

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        report.Should().Contain("5,000.00 PLN (GAIN)");
    }

    [Fact]
    public void GenerateReport_WithLoss_DisplaysLossLabel()
    {
        var taxSummary = CreateSimpleTaxSummary();
        var calculation = taxSummary.Calculations[0];
        calculation.GainLossPLN = -2000m;

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        report.Should().Contain("-2,000.00 PLN (LOSS)");
    }

    [Fact]
    public void GenerateReport_WithWarnings_DisplaysWarningsSection()
    {
        var taxSummary = CreateSimpleTaxSummary();
        taxSummary.Warnings.Add("ISIN US0378331005: 10 shares remain unsold");
        taxSummary.Warnings.Add("ISIN US5949181045: 5 shares remain unsold");

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        report.Should().Contain("WARNINGS");
        report.Should().Contain("ISIN US0378331005: 10 shares remain unsold");
        report.Should().Contain("ISIN US5949181045: 5 shares remain unsold");
    }

    [Fact]
    public void GenerateReport_WithoutWarnings_DoesNotDisplayWarningsSection()
    {
        var taxSummary = CreateSimpleTaxSummary();
        taxSummary.Warnings.Clear();

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        var warningsIndex = report.IndexOf("WARNINGS", StringComparison.Ordinal);
        warningsIndex.Should().BeLessThan(0);
    }

    [Fact]
    public void GenerateReport_WithMultipleTransactions_NumbersThemSequentially()
    {
        var taxSummary = new TaxSummary
        {
            Year = 2024,
            Calculations = new List<TaxCalculation>
            {
                CreateTaxCalculation("US0378331005", new DateTime(2024, 1, 15)),
                CreateTaxCalculation("US0378331005", new DateTime(2024, 2, 15)),
                CreateTaxCalculation("US0378331005", new DateTime(2024, 3, 15))
            }
        };

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        report.Should().Contain("Transaction #1");
        report.Should().Contain("Transaction #2");
        report.Should().Contain("Transaction #3");
    }

    [Fact]
    public void GenerateReport_WithMultipleISINs_GroupsByISIN()
    {
        var taxSummary = new TaxSummary
        {
            Year = 2024,
            Calculations = new List<TaxCalculation>
            {
                CreateTaxCalculation("US0378331005", new DateTime(2024, 1, 15)),
                CreateTaxCalculation("US5949181045", new DateTime(2024, 2, 15)),
                CreateTaxCalculation("US0378331005", new DateTime(2024, 3, 15))
            }
        };

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        report.Should().Contain("ISIN: US0378331005");
        report.Should().Contain("ISIN: US5949181045");
        
        var firstISINIndex = report.IndexOf("ISIN: US0378331005", StringComparison.Ordinal);
        var secondISINIndex = report.IndexOf("ISIN: US5949181045", StringComparison.Ordinal);
        var thirdISINIndex = report.IndexOf("ISIN: US0378331005", firstISINIndex + 1, StringComparison.Ordinal);
        
        thirdISINIndex.Should().BeLessThan(0);
    }

    [Fact]
    public void GenerateReport_NullSummary_ThrowsArgumentNullException()
    {
        var act = () => _generator.GenerateReport(null!, "test-job-123", "test@example.com");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GenerateReport_EmptyCalculations_GeneratesReportWithZeroValues()
    {
        var taxSummary = new TaxSummary
        {
            Year = 2024,
            Calculations = new List<TaxCalculation>()
        };

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        report.Should().NotBeNullOrEmpty();
        report.Should().Contain("0.00 PLN");
        report.Should().MatchRegex(@"Matched Transactions:\s+0");
    }

    [Fact]
    public void GenerateReport_ContainsMethodologyExplanation()
    {
        var taxSummary = CreateSimpleTaxSummary();

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        report.Should().Contain("FIFO (First In, First Out)");
        report.Should().Contain("National Bank of Poland (NBP)");
        report.Should().Contain("19%");
    }

    [Fact]
    public void GenerateReport_ContainsDisclaimer()
    {
        var taxSummary = CreateSimpleTaxSummary();

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        report.Should().Contain("informational purposes only");
        report.Should().Contain("tax advice");
        report.Should().Contain("qualified tax professional");
        report.Should().Contain("PIT-38");
    }

    [Fact]
    public void GenerateReport_FormatsTimestampCorrectly()
    {
        var taxSummary = CreateSimpleTaxSummary();

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        report.Should().MatchRegex(@"Report Generated: \d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2} UTC");
    }

    [Fact]
    public void GenerateReport_LargeNumbers_FormatsWithThousandsSeparators()
    {
        var taxSummary = CreateSimpleTaxSummary();
        taxSummary.TotalGainsPLN = 1234567.89m;

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        report.Should().Contain("1,234,567.89");
    }

    [Fact]
    public void GenerateReport_DecimalPrecision_MaintainsCorrectDecimalPlaces()
    {
        var taxSummary = CreateSimpleTaxSummary();
        var calculation = taxSummary.Calculations[0];
        calculation.SellTransaction.Shares = 10.123456m;
        calculation.SellTransaction.PricePerShare = 150.1234m;

        var report = _generator.GenerateReport(taxSummary, "test-job-123", "test@example.com");

        report.Should().Contain("10.123456");
        report.Should().Contain("150.1234");
    }

    private static TaxSummary CreateSimpleTaxSummary()
    {
        var sellTx = CreateTransaction(TransactionAction.Sell, 100m, 150m, new DateTime(2024, 6, 15));
        sellTx.Ticker = "TEST";
        sellTx.Name = "Test Corp";

        return new TaxSummary
        {
            Year = 2024,
            TotalGainsPLN = 5000m,
            TotalLossesPLN = 0m,
            NetTaxableAmountPLN = 5000m,
            TotalTransactions = 1,
            Calculations = new List<TaxCalculation>
            {
                new()
                {
                    ISIN = "US0000000000",
                    SellTransaction = sellTx,
                    CostBasisPLN = 10000m,
                    ProceedsPLN = 15000m,
                    GainLossPLN = 5000m,
                    MatchedBuys = new List<MatchedBuy>()
                }
            }
        };
    }

    private static TaxCalculation CreateTaxCalculation(string isin, DateTime sellDate)
    {
        var sellTx = CreateTransaction(TransactionAction.Sell, 10m, 150m, sellDate);
        sellTx.Ticker = "TEST";
        sellTx.Name = "Test Corp";

        return new TaxCalculation
        {
            ISIN = isin,
            SellTransaction = sellTx,
            CostBasisPLN = 1000m,
            ProceedsPLN = 1500m,
            GainLossPLN = 500m,
            MatchedBuys = new List<MatchedBuy>()
        };
    }

    private static Transaction CreateTransaction(TransactionAction action, decimal shares, decimal price, DateTime time)
    {
        return new Transaction
        {
            Action = action,
            Time = time,
            ISIN = "US0000000000",
            Ticker = "TEST",
            Name = "Test Corp",
            TransactionId = Guid.NewGuid().ToString(),
            Shares = shares,
            PricePerShare = price,
            PriceCurrency = Currency.USD,
            BrokerExchangeRate = 4.0m,
            Result = shares * price,
            ResultCurrency = Currency.USD,
            Total = shares * price,
            TotalCurrency = Currency.USD
        };
    }
}
