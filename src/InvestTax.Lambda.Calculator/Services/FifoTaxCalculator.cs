using InvestTax.Core.Enums;
using InvestTax.Core.Interfaces;
using InvestTax.Core.Models;
using Microsoft.Extensions.Logging;

namespace InvestTax.Lambda.Calculator.Services;

/// <summary>
/// FIFO (First In, First Out) tax calculation engine for capital gains
/// </summary>
public class FifoTaxCalculator : ITaxCalculator
{
    private readonly ILogger<FifoTaxCalculator> _logger;

    public FifoTaxCalculator(ILogger<FifoTaxCalculator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculate taxes for all transactions using FIFO methodology
    /// </summary>
    /// <param name="transactions">All buy and sell transactions</param>
    /// <param name="year">Tax year</param>
    /// <returns>Complete tax summary</returns>
    public TaxSummary CalculateTaxes(List<Transaction> transactions, int year)
    {
        if (transactions is null)
        {
            throw new ArgumentNullException(nameof(transactions));
        }

        _logger.LogInformation("Starting FIFO tax calculation for {Count} transactions in year {Year}", 
            transactions.Count, year);

        var summary = new TaxSummary
        {
            Year = year
        };

        // Group transactions by ISIN
        var groupedByIsin = transactions
            .GroupBy(t => t.ISIN)
            .OrderBy(g => g.Key);

        foreach (var group in groupedByIsin)
        {
            var isin = group.Key;
            var isinTransactions = group.OrderBy(t => t.Time).ToList();

            _logger.LogInformation("Processing ISIN: {ISIN} with {Count} transactions", 
                isin, isinTransactions.Count);

            ProcessIsinTransactions(isin, isinTransactions, summary);
        }

        // Calculate totals
        summary.TotalGainsPLN = summary.Calculations
            .Where(c => c.IsGain)
            .Sum(c => c.GainLossPLN);

        summary.TotalLossesPLN = Math.Abs(summary.Calculations
            .Where(c => !c.IsGain)
            .Sum(c => c.GainLossPLN));

        summary.NetTaxableAmountPLN = summary.TotalGainsPLN - summary.TotalLossesPLN;
        summary.TotalTransactions = summary.Calculations.Count;

        _logger.LogInformation(
            "Tax calculation complete. Gains: {Gains:N2} PLN, Losses: {Losses:N2} PLN, Net: {Net:N2} PLN, Tax: {Tax:N2} PLN",
            summary.TotalGainsPLN, summary.TotalLossesPLN, 
            summary.NetTaxableAmountPLN, summary.EstimatedTaxPLN);

        return summary;
    }

    /// <summary>
    /// Process all transactions for a single ISIN using FIFO matching
    /// </summary>
    private void ProcessIsinTransactions(string isin, List<Transaction> transactions, TaxSummary summary)
    {
        // FIFO queue: oldest buys at the front
        var buyQueue = new Queue<BuyPosition>();

        foreach (var transaction in transactions)
        {
            if (transaction.Action == TransactionAction.Buy)
            {
                // Add to FIFO queue
                buyQueue.Enqueue(new BuyPosition
                {
                    Transaction = transaction,
                    RemainingShares = transaction.Shares
                });

                _logger.LogDebug("Added BUY to queue: {Shares} shares at {Price} {Currency} on {Date}",
                    transaction.Shares, transaction.PricePerShare, 
                    transaction.PriceCurrency, transaction.Time.Date);
            }
            else if (transaction.Action == TransactionAction.Sell)
            {
                // Match against oldest buys (FIFO)
                var calculation = MatchSellToBuilds(isin, transaction, buyQueue);
                summary.Calculations.Add(calculation);

                _logger.LogDebug("Processed SELL: {Shares} shares, Gain/Loss: {GainLoss:N2} PLN",
                    transaction.Shares, calculation.GainLossPLN);
            }
        }

        // Check for remaining positions (unsold shares)
        if (buyQueue.Count > 0)
        {
            var remainingShares = buyQueue.Sum(b => b.RemainingShares);
            summary.Warnings.Add(
                $"ISIN {isin}: {remainingShares:N6} shares remain unsold (still held)");
            
            _logger.LogInformation("ISIN {ISIN}: {Shares} shares remain unsold", 
                isin, remainingShares);
        }
    }

    /// <summary>
    /// Match a sell transaction to buy transactions using FIFO
    /// </summary>
    private TaxCalculation MatchSellToBuilds(string isin, Transaction sellTransaction, Queue<BuyPosition> buyQueue)
    {
        var calculation = new TaxCalculation
        {
            ISIN = isin,
            SellTransaction = sellTransaction
        };

        var remainingSharesToSell = sellTransaction.Shares;
        
        // Convert sell price to PLN using the exchange rate
        var sellPricePerSharePLN = ConvertToPLN(
            sellTransaction.PricePerShare, 
            sellTransaction.PriceCurrency,
            sellTransaction.BrokerExchangeRate ?? 1m);

        while (remainingSharesToSell > 0m && buyQueue.Count > 0)
        {
            var oldestBuy = buyQueue.Peek();
            var sharesToMatch = Math.Min(remainingSharesToSell, oldestBuy.RemainingShares);

            // Calculate cost basis in PLN for matched shares
            var buyPricePerSharePLN = ConvertToPLN(
                oldestBuy.Transaction.PricePerShare,
                oldestBuy.Transaction.PriceCurrency,
                oldestBuy.Transaction.BrokerExchangeRate ?? 1m);

            var costBasisPLN = sharesToMatch * buyPricePerSharePLN;
            var proceedsPLN = sharesToMatch * sellPricePerSharePLN;

            calculation.MatchedBuys.Add(new MatchedBuy
            {
                BuyTransaction = oldestBuy.Transaction,
                SharesMatched = sharesToMatch,
                CostBasisPLN = Math.Round(costBasisPLN, 2)
            });

            calculation.CostBasisPLN += Math.Round(costBasisPLN, 2);
            calculation.ProceedsPLN += Math.Round(proceedsPLN, 2);

            // Update remaining shares
            oldestBuy.RemainingShares -= sharesToMatch;
            remainingSharesToSell -= sharesToMatch;

            // Remove from queue if fully matched
            if (oldestBuy.RemainingShares == 0m)
            {
                buyQueue.Dequeue();
            }

            _logger.LogDebug("Matched {Shares} shares: Cost {Cost:N2} PLN, Proceeds {Proceeds:N2} PLN",
                sharesToMatch, costBasisPLN, proceedsPLN);
        }

        // Check if we couldn't match all shares (error condition)
        if (remainingSharesToSell > 0m)
        {
            var errorMessage = $"Cannot sell {remainingSharesToSell} shares of {isin} - insufficient buy positions";
            _logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        // Calculate gain/loss
        calculation.GainLossPLN = calculation.ProceedsPLN - calculation.CostBasisPLN;

        return calculation;
    }

    /// <summary>
    /// Convert amount to PLN using the provided exchange rate
    /// </summary>
    private decimal ConvertToPLN(decimal amount, Currency currency, decimal exchangeRate)
    {
        // If already PLN, return as-is
        if (currency == Currency.PLN)
        {
            return amount;
        }

        // Apply exchange rate
        return amount * exchangeRate;
    }

    /// <summary>
    /// Represents a buy position in the FIFO queue
    /// </summary>
    private class BuyPosition
    {
        public Transaction Transaction { get; set; } = null!;
        public decimal RemainingShares { get; set; }
    }
}
