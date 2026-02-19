using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using InvestTax.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace InvestTax.Infrastructure.AWS;

public class EmailService : IEmailService
{
    private readonly IAmazonSimpleEmailService _sesClient;
    private readonly string _fromEmail;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IAmazonSimpleEmailService sesClient,
        string fromEmail,
        ILogger<EmailService> logger)
    {
        _sesClient = sesClient;
        _fromEmail = fromEmail;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string bodyText,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending email to {Email}: {Subject}", toEmail, subject);

        var request = new SendEmailRequest
        {
            Source = _fromEmail,
            Destination = new Destination
            {
                ToAddresses = new List<string> { toEmail }
            },
            Message = new Message
            {
                Subject = new Content(subject),
                Body = new Body
                {
                    Text = new Content(bodyText)
                }
            }
        };

        try
        {
            var response = await _sesClient.SendEmailAsync(request, cancellationToken);
            _logger.LogInformation("Email sent successfully. MessageId: {MessageId}", response.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }

    public async Task SendTaxReportEmailAsync(
        string toEmail,
        string reportContent,
        string jobId,
        CancellationToken cancellationToken = default)
    {
        var subject = $"InvestTax Calculator - Your Tax Report (Job: {jobId})";
        var body = $@"Your tax calculation has completed successfully.

{reportContent}

---
Disclaimer: This calculation is for informational purposes only. Please consult with a tax professional before filing.

Job ID: {jobId}
Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
";

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendErrorEmailAsync(
        string toEmail,
        string errorMessage,
        string jobId,
        CancellationToken cancellationToken = default)
    {
        var subject = $"InvestTax Calculator - Processing Error (Job: {jobId})";
        var body = $@"We encountered an error processing your tax calculation.

Error: {errorMessage}

Please verify your CSV file format and try again. If the problem persists, contact support.

Job ID: {jobId}
Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
";

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }
}
