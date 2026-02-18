using InvestTax.Lambda.EmailSender.Models;

namespace InvestTax.Lambda.EmailSender.Services;

/// <summary>
/// Service for generating email templates (HTML and plain text)
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Generate success email with tax report
    /// </summary>
    /// <param name="recipientEmail">Recipient email address</param>
    /// <param name="year">Tax year</param>
    /// <param name="jobId">Job ID</param>
    /// <param name="reportContent">Plain text report content</param>
    /// <returns>Email content with subject and body</returns>
    EmailContent GenerateSuccessEmail(
        string recipientEmail, 
        int year, 
        string jobId, 
        string reportContent);

    /// <summary>
    /// Generate error email
    /// </summary>
    /// <param name="recipientEmail">Recipient email address</param>
    /// <param name="year">Tax year</param>
    /// <param name="jobId">Job ID</param>
    /// <param name="errorStage">Stage where error occurred</param>
    /// <param name="errorMessage">Error message</param>
    /// <returns>Email content with subject and body</returns>
    EmailContent GenerateErrorEmail(
        string recipientEmail,
        int year,
        string jobId,
        string errorStage,
        string errorMessage);
}
