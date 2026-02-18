using InvestTax.Lambda.EmailSender.Models;

namespace InvestTax.Lambda.EmailSender.Services;

/// <summary>
/// Service for generating email templates (HTML and plain text)
/// </summary>
public class EmailTemplateService : IEmailTemplateService
{
    /// <summary>
    /// Generate success email with tax report
    /// </summary>
    public EmailContent GenerateSuccessEmail(
        string recipientEmail, 
        int year, 
        string jobId, 
        string reportContent)
    {
        var subject = $"InvestTax Calculator - Tax Report {year} (Job: {jobId[..8]}...)";
        
        return new EmailContent
        {
            Subject = subject,
            HtmlBody = EmailTemplates.GenerateSuccessHtml(year, jobId, reportContent),
            TextBody = EmailTemplates.GenerateSuccessText(year, jobId, reportContent)
        };
    }

    /// <summary>
    /// Generate error email
    /// </summary>
    public EmailContent GenerateErrorEmail(
        string recipientEmail,
        int year,
        string jobId,
        string errorStage,
        string errorMessage)
    {
        var subject = $"InvestTax Calculator - Processing Failed (Job: {jobId[..8]}...)";
        
        return new EmailContent
        {
            Subject = subject,
            HtmlBody = EmailTemplates.GenerateErrorHtml(year, jobId, errorStage, errorMessage),
            TextBody = EmailTemplates.GenerateErrorText(year, jobId, errorStage, errorMessage)
        };
    }
}
