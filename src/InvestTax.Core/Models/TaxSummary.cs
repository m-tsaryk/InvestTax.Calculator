namespace InvestTax.Core.Models;

/// <summary>
/// Summary of all tax calculations for the year
/// </summary>
public class TaxSummary
{
    /// <summary>Tax year</summary>
    public int Year { get; set; }
    
    /// <summary>All individual calculations</summary>
    public List<TaxCalculation> Calculations { get; set; } = new();
    
    /// <summary>Total capital gains in PLN</summary>
    public decimal TotalGainsPLN { get; set; }
    
    /// <summary>Total capital losses in PLN</summary>
    public decimal TotalLossesPLN { get; set; }
    
    /// <summary>Net taxable amount in PLN</summary>
    public decimal NetTaxableAmountPLN { get; set; }
    
    /// <summary>Tax owed at 19% (informational)</summary>
    public decimal EstimatedTaxPLN => NetTaxableAmountPLN * 0.19m;
    
    /// <summary>Number of taxable transactions</summary>
    public int TotalTransactions { get; set; }
    
    /// <summary>Warning messages</summary>
    public List<string> Warnings { get; set; } = new();
}
