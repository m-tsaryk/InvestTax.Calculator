namespace InvestTax.Core.Interfaces;

/// <summary>
/// Email sending service
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string bodyText, CancellationToken cancellationToken = default);
    Task SendTaxReportEmailAsync(string toEmail, string reportContent, string jobId, CancellationToken cancellationToken = default);
    Task SendErrorEmailAsync(string toEmail, string errorMessage, string jobId, CancellationToken cancellationToken = default);
}
