namespace InvestTax.Lambda.EmailSender.Models;

/// <summary>
/// Email content with subject and body in multiple formats
/// </summary>
public sealed class EmailContent
{
    /// <summary>
    /// Email subject line
    /// </summary>
    public required string Subject { get; init; }

    /// <summary>
    /// HTML body content
    /// </summary>
    public required string HtmlBody { get; init; }

    /// <summary>
    /// Plain text body content
    /// </summary>
    public required string TextBody { get; init; }
}
