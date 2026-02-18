using FluentValidation;
using InvestTax.Lambda.Validator.Models;

namespace InvestTax.Lambda.Validator.Validators;

/// <summary>
/// Validates individual CSV rows using FluentValidation rules
/// </summary>
public class CsvRowValidator : AbstractValidator<CsvRow>
{
    private static readonly string[] ValidActions = { "Market buy", "Market sell" };
    
    public CsvRowValidator()
    {
        RuleFor(x => x.Action)
            .NotEmpty()
            .Must(a => ValidActions.Contains(a))
            .WithMessage("Action must be 'Market buy' or 'Market sell'");
        
        RuleFor(x => x.Time)
            .NotEmpty()
            .Must(BeValidDateTime)
            .WithMessage("Time must be valid ISO 8601 datetime");
        
        RuleFor(x => x.ISIN)
            .NotEmpty()
            .Length(12)
            .Matches("^[A-Z0-9]{12}$")
            .WithMessage("ISIN must be 12 alphanumeric characters");
        
        RuleFor(x => x.NoOfShares)
            .NotEmpty()
            .Must(BePositiveDecimal)
            .WithMessage("No. of shares must be positive number");
        
        RuleFor(x => x.PricePerShare)
            .NotEmpty()
            .Must(BePositiveDecimal)
            .WithMessage("Price per share must be positive number");
        
        RuleFor(x => x.CurrencySymbol)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be 3-letter ISO code");
    }
    
    private bool BeValidDateTime(string dateTime)
    {
        return DateTime.TryParse(dateTime, out _);
    }
    
    private bool BePositiveDecimal(string value)
    {
        return decimal.TryParse(value, out var result) && result > 0;
    }
}
