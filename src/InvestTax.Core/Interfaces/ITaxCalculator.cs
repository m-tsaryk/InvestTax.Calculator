using InvestTax.Core.Models;

namespace InvestTax.Core.Interfaces;

/// <summary>
/// FIFO tax calculation service
/// </summary>
public interface ITaxCalculator
{
    /// <summary>
    /// Calculate taxes for all transactions using FIFO methodology
    /// </summary>
    /// <param name="transactions">All buy and sell transactions</param>
    /// <param name="year">Tax year</param>
    /// <returns>Complete tax summary</returns>
    TaxSummary CalculateTaxes(List<Transaction> transactions, int year);
}
