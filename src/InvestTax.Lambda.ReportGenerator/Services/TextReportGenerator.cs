using InvestTax.Core.Models;
using System.Text;

namespace InvestTax.Lambda.ReportGenerator.Services;

/// <summary>
/// Service for generating plain text tax reports
/// </summary>
public class TextReportGenerator
{
    private const int ReportWidth = 80;
    private const string HeaderLine = "═══════════════════════════════════════════════════════════════════════════════";
    private const string SectionLine = "───────────────────────────────────────────────────────────────────────────────";
    private const string ThinLine = "···············································································";

    /// <summary>
    /// Generate a formatted plain text report from tax calculation summary
    /// </summary>
    /// <param name="taxSummary">Tax calculation summary</param>
    /// <param name="jobId">Job ID</param>
    /// <param name="email">User email</param>
    /// <returns>Formatted plain text report</returns>
    public string GenerateReport(TaxSummary taxSummary, string jobId, string email)
    {
        ArgumentNullException.ThrowIfNull(taxSummary);

        var report = new StringBuilder();

        // Header
        AppendHeader(report, jobId, taxSummary.Year);

        // Summary Section
        AppendSummarySection(report, taxSummary);

        // Detailed Transactions Section
        AppendTransactionsSection(report, taxSummary);

        // Warnings Section
        if (taxSummary.Warnings.Count > 0)
        {
            AppendWarningsSection(report, taxSummary);
        }

        // Methodology and Disclaimer
        AppendMethodologySection(report);
        AppendDisclaimerSection(report);

        // Footer
        AppendFooter(report, email);

        return report.ToString();
    }

    /// <summary>
    /// Append report header
    /// </summary>
    private void AppendHeader(StringBuilder report, string jobId, int year)
    {
        report.AppendLine(HeaderLine);
        report.AppendLine(CenterText("POLISH CAPITAL GAINS TAX CALCULATION (PIT-38)"));
        report.AppendLine(HeaderLine);
        report.AppendLine();
        report.AppendLine($"Report Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        report.AppendLine($"Job ID: {jobId}");
        report.AppendLine($"Tax Year: {year}");
        report.AppendLine();
    }

    /// <summary>
    /// Append summary section
    /// </summary>
    private void AppendSummarySection(StringBuilder report, TaxSummary taxSummary)
    {
        report.AppendLine(SectionLine);
        report.AppendLine(CenterText("SUMMARY"));
        report.AppendLine(SectionLine);
        report.AppendLine();
        
        report.AppendLine($"  {"Total Capital Gains:",-40} {taxSummary.TotalGainsPLN,15:N2} PLN");
        report.AppendLine($"  {"Total Capital Losses:",-40} {taxSummary.TotalLossesPLN,15:N2} PLN");
        report.AppendLine($"  {"Net Taxable Amount:",-40} {taxSummary.NetTaxableAmountPLN,15:N2} PLN");
        report.AppendLine($"  {"Estimated Tax (19%):",-40} {taxSummary.EstimatedTaxPLN,15:N2} PLN");
        report.AppendLine($"  {"Matched Transactions:",-40} {taxSummary.TotalTransactions,15}");
        report.AppendLine();
    }

    /// <summary>
    /// Append detailed transactions section
    /// </summary>
    private void AppendTransactionsSection(StringBuilder report, TaxSummary taxSummary)
    {
        report.AppendLine(SectionLine);
        report.AppendLine(CenterText("DETAILED TRANSACTIONS"));
        report.AppendLine(SectionLine);
        report.AppendLine();

        // Group by ISIN
        var groupedByIsin = taxSummary.Calculations
            .GroupBy(c => c.ISIN)
            .OrderBy(g => g.Key);

        var transactionNumber = 1;

        foreach (var isinGroup in groupedByIsin)
        {
            var firstCalc = isinGroup.First();
            var ticker = firstCalc.SellTransaction.Ticker ?? "N/A";
            var name = firstCalc.SellTransaction.Name ?? "N/A";

            report.AppendLine($"ISIN: {isinGroup.Key}");
            report.AppendLine($"Ticker: {ticker} | Name: {name}");
            report.AppendLine(ThinLine);

            foreach (var calc in isinGroup.OrderBy(c => c.SellTransaction.Time))
            {
                AppendTransactionDetail(report, calc, transactionNumber);
                transactionNumber++;
                report.AppendLine();
            }
        }
    }

    /// <summary>
    /// Append individual transaction detail
    /// </summary>
    private void AppendTransactionDetail(StringBuilder report, TaxCalculation calc, int number)
    {
        var sellTx = calc.SellTransaction;
        
        report.AppendLine($"Transaction #{number}");
        report.AppendLine($"  Sell Date:        {sellTx.Time:yyyy-MM-dd}");
        report.AppendLine($"  Shares Sold:      {sellTx.Shares:N6}");
        report.AppendLine($"  Sell Price:       {sellTx.PricePerShare:N4} {sellTx.PriceCurrency}");
        report.AppendLine($"  Proceeds (PLN):   {calc.ProceedsPLN:N2}");
        report.AppendLine();
        
        report.AppendLine("  Matched Buys (FIFO):");
        
        foreach (var matchedBuy in calc.MatchedBuys)
        {
            var buyTx = matchedBuy.BuyTransaction;
            report.AppendLine($"    • {buyTx.Time:yyyy-MM-dd}: {matchedBuy.SharesMatched:N6} shares @ {buyTx.PricePerShare:N4} {buyTx.PriceCurrency}");
            report.AppendLine($"      Cost Basis (PLN): {matchedBuy.CostBasisPLN:N2}");
        }
        
        report.AppendLine();
        report.AppendLine($"  Total Cost Basis: {calc.CostBasisPLN:N2} PLN");
        report.AppendLine($"  Total Proceeds:   {calc.ProceedsPLN:N2} PLN");
        report.AppendLine($"  Gain/Loss:        {calc.GainLossPLN:N2} PLN ({(calc.IsGain ? "GAIN" : "LOSS")})");
    }

    /// <summary>
    /// Append warnings section
    /// </summary>
    private void AppendWarningsSection(StringBuilder report, TaxSummary taxSummary)
    {
        report.AppendLine(SectionLine);
        report.AppendLine(CenterText("WARNINGS"));
        report.AppendLine(SectionLine);
        report.AppendLine();

        foreach (var warning in taxSummary.Warnings)
        {
            report.AppendLine($"  ⚠ {warning}");
        }

        report.AppendLine();
    }

    /// <summary>
    /// Append methodology section
    /// </summary>
    private void AppendMethodologySection(StringBuilder report)
    {
        report.AppendLine(SectionLine);
        report.AppendLine(CenterText("CALCULATION METHODOLOGY"));
        report.AppendLine(SectionLine);
        report.AppendLine();
        
        report.AppendLine("This report uses the FIFO (First In, First Out) cost basis method for matching");
        report.AppendLine("sell transactions to buy transactions. This means:");
        report.AppendLine();
        report.AppendLine("• The oldest shares purchased are considered sold first");
        report.AppendLine("• Exchange rates are sourced from the National Bank of Poland (NBP)");
        report.AppendLine("• All amounts are converted to PLN using official NBP rates");
        report.AppendLine("• Capital gains tax rate: 19% (applicable in Poland)");
        report.AppendLine("• Losses can be offset against gains");
        report.AppendLine();
    }

    /// <summary>
    /// Append disclaimer section
    /// </summary>
    private void AppendDisclaimerSection(StringBuilder report)
    {
        report.AppendLine(SectionLine);
        report.AppendLine(CenterText("DISCLAIMER"));
        report.AppendLine(SectionLine);
        report.AppendLine();
        
        report.AppendLine("This report is provided for informational purposes only and does not constitute");
        report.AppendLine("tax advice. Please consult with a qualified tax professional or accountant to");
        report.AppendLine("review your specific tax situation and ensure compliance with Polish tax law.");
        report.AppendLine();
        report.AppendLine("The calculations are based on exchange rates from the National Bank of Poland");
        report.AppendLine("(NBP) and the transaction data you provided. Please verify all information");
        report.AppendLine("before filing your tax return (PIT-38).");
        report.AppendLine();
    }

    /// <summary>
    /// Append report footer
    /// </summary>
    private void AppendFooter(StringBuilder report, string email)
    {
        report.AppendLine(HeaderLine);
        report.AppendLine(CenterText("END OF REPORT"));
        report.AppendLine(HeaderLine);
        report.AppendLine();
        report.AppendLine($"Report generated for: {email}");
        report.AppendLine("InvestTax Calculator - Polish Capital Gains Tax Calculation Tool");
        report.AppendLine();
    }

    /// <summary>
    /// Center text within the report width
    /// </summary>
    private string CenterText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var padding = (ReportWidth - text.Length) / 2;
        
        if (padding <= 0)
        {
            return text;
        }

        return new string(' ', padding) + text;
    }
}
