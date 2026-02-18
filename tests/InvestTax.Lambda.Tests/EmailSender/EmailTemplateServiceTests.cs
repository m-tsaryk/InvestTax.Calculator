using InvestTax.Lambda.EmailSender.Models;
using InvestTax.Lambda.EmailSender.Services;

namespace InvestTax.Lambda.Tests.EmailSender;

public class EmailTemplateServiceTests
{
    private readonly EmailTemplateService _service;

    public EmailTemplateServiceTests()
    {
        _service = new EmailTemplateService();
    }

    [Fact]
    public void GenerateSuccessEmail_ValidInput_ReturnsEmailWithSubject()
    {
        var email = "user@example.com";
        var year = 2024;
        var jobId = "550e8400-e29b-41d4-a716-446655440000";
        var reportContent = "Test report content";

        var result = _service.GenerateSuccessEmail(email, year, jobId, reportContent);

        Assert.NotNull(result.Subject);
        Assert.Contains("2024", result.Subject);
        Assert.Contains("550e8400", result.Subject);
    }

    [Fact]
    public void GenerateSuccessEmail_ValidInput_ReturnsHtmlBodyWithReportContent()
    {
        var email = "user@example.com";
        var year = 2024;
        var jobId = "550e8400-e29b-41d4-a716-446655440000";
        var reportContent = "Sample tax report\nTotal Gains: 1000 PLN";

        var result = _service.GenerateSuccessEmail(email, year, jobId, reportContent);

        Assert.Contains("Tax Calculation Complete", result.HtmlBody);
        Assert.Contains("2024", result.HtmlBody);
        Assert.Contains(jobId, result.HtmlBody);
        Assert.Contains("Sample tax report", result.HtmlBody);
        Assert.Contains("Total Gains: 1000 PLN", result.HtmlBody);
    }

    [Fact]
    public void GenerateSuccessEmail_ValidInput_ReturnsTextBodyWithReportContent()
    {
        var email = "user@example.com";
        var year = 2024;
        var jobId = "550e8400-e29b-41d4-a716-446655440000";
        var reportContent = "Sample tax report\nTotal Gains: 1000 PLN";

        var result = _service.GenerateSuccessEmail(email, year, jobId, reportContent);

        Assert.Contains("TAX CALCULATION COMPLETE", result.TextBody);
        Assert.Contains("2024", result.TextBody);
        Assert.Contains(jobId, result.TextBody);
        Assert.Contains(reportContent, result.TextBody);
    }

    [Fact]
    public void GenerateSuccessEmail_HtmlSpecialCharacters_EscapesCorrectly()
    {
        var email = "user@example.com";
        var year = 2024;
        var jobId = "test-job-id";
        var reportContent = "<script>alert('test')</script>";

        var result = _service.GenerateSuccessEmail(email, year, jobId, reportContent);

        Assert.DoesNotContain("<script>", result.HtmlBody);
        Assert.Contains("&lt;script&gt;", result.HtmlBody);
    }

    [Fact]
    public void GenerateSuccessEmail_ValidInput_IncludesNextSteps()
    {
        var email = "user@example.com";
        var year = 2024;
        var jobId = "test-job-id";
        var reportContent = "Report content";

        var result = _service.GenerateSuccessEmail(email, year, jobId, reportContent);

        Assert.Contains("Next Steps", result.HtmlBody);
        Assert.Contains("Review the calculations", result.HtmlBody);
        Assert.Contains("NEXT STEPS", result.TextBody);
    }

    [Fact]
    public void GenerateSuccessEmail_ValidInput_IncludesDisclaimer()
    {
        var email = "user@example.com";
        var year = 2024;
        var jobId = "test-job-id";
        var reportContent = "Report content";

        var result = _service.GenerateSuccessEmail(email, year, jobId, reportContent);

        Assert.Contains("Disclaimer", result.HtmlBody);
        Assert.Contains("does not constitute tax advice", result.HtmlBody);
        Assert.Contains("DISCLAIMER", result.TextBody);
    }

    [Fact]
    public void GenerateErrorEmail_ValidInput_ReturnsEmailWithSubject()
    {
        var email = "user@example.com";
        var year = 2024;
        var jobId = "550e8400-e29b-41d4-a716-446655440000";
        var errorStage = "Validation";
        var errorMessage = "Invalid CSV format";

        var result = _service.GenerateErrorEmail(email, year, jobId, errorStage, errorMessage);

        Assert.NotNull(result.Subject);
        Assert.Contains("Failed", result.Subject);
        Assert.Contains("550e8400", result.Subject);
    }

    [Fact]
    public void GenerateErrorEmail_ValidInput_ReturnsHtmlBodyWithErrorDetails()
    {
        var email = "user@example.com";
        var year = 2024;
        var jobId = "550e8400-e29b-41d4-a716-446655440000";
        var errorStage = "Validation";
        var errorMessage = "Invalid CSV format detected in row 5";

        var result = _service.GenerateErrorEmail(email, year, jobId, errorStage, errorMessage);

        Assert.Contains("Tax Calculation Failed", result.HtmlBody);
        Assert.Contains("2024", result.HtmlBody);
        Assert.Contains(jobId, result.HtmlBody);
        Assert.Contains("Validation", result.HtmlBody);
        Assert.Contains("Invalid CSV format detected in row 5", result.HtmlBody);
    }

    [Fact]
    public void GenerateErrorEmail_ValidInput_ReturnsTextBodyWithErrorDetails()
    {
        var email = "user@example.com";
        var year = 2024;
        var jobId = "550e8400-e29b-41d4-a716-446655440000";
        var errorStage = "NBP Rate Fetching";
        var errorMessage = "Failed to fetch exchange rate for 2024-01-15";

        var result = _service.GenerateErrorEmail(email, year, jobId, errorStage, errorMessage);

        Assert.Contains("PROCESSING FAILED", result.TextBody);
        Assert.Contains("2024", result.TextBody);
        Assert.Contains(jobId, result.TextBody);
        Assert.Contains("NBP Rate Fetching", result.TextBody);
        Assert.Contains("Failed to fetch exchange rate", result.TextBody);
    }

    [Fact]
    public void GenerateErrorEmail_HtmlSpecialCharacters_EscapesCorrectly()
    {
        var email = "user@example.com";
        var year = 2024;
        var jobId = "test-job-id";
        var errorStage = "<script>Stage</script>";
        var errorMessage = "<img src=x onerror=alert(1)>";

        var result = _service.GenerateErrorEmail(email, year, jobId, errorStage, errorMessage);

        Assert.DoesNotContain("<script>", result.HtmlBody);
        Assert.DoesNotContain("<img src=x", result.HtmlBody);
        Assert.Contains("&lt;script&gt;", result.HtmlBody);
        Assert.Contains("&lt;img", result.HtmlBody);
    }

    [Fact]
    public void GenerateErrorEmail_ValidInput_IncludesTroubleshootingSteps()
    {
        var email = "user@example.com";
        var year = 2024;
        var jobId = "test-job-id";
        var errorStage = "Validation";
        var errorMessage = "Error occurred";

        var result = _service.GenerateErrorEmail(email, year, jobId, errorStage, errorMessage);

        Assert.Contains("What to do next", result.HtmlBody);
        Assert.Contains("CSV file", result.HtmlBody);
        Assert.Contains("WHAT TO DO NEXT", result.TextBody);
    }

    [Fact]
    public void GenerateErrorEmail_ValidInput_IncludesCommonIssues()
    {
        var email = "user@example.com";
        var year = 2024;
        var jobId = "test-job-id";
        var errorStage = "Validation";
        var errorMessage = "Error occurred";

        var result = _service.GenerateErrorEmail(email, year, jobId, errorStage, errorMessage);

        Assert.Contains("Common Issues", result.HtmlBody);
        Assert.Contains("Validation errors", result.HtmlBody);
        Assert.Contains("COMMON ISSUES", result.TextBody);
    }

    [Fact]
    public void GenerateSuccessEmail_LongReportContent_HandlesCorrectly()
    {
        var email = "user@example.com";
        var year = 2024;
        var jobId = "test-job-id";
        var reportContent = string.Join("\n", Enumerable.Repeat("Transaction line with details", 1000));

        var result = _service.GenerateSuccessEmail(email, year, jobId, reportContent);

        Assert.NotNull(result.Subject);
        Assert.NotNull(result.HtmlBody);
        Assert.NotNull(result.TextBody);
        Assert.Contains(reportContent, result.HtmlBody);
        Assert.Contains(reportContent, result.TextBody);
    }

    [Fact]
    public void GenerateErrorEmail_LongErrorMessage_HandlesCorrectly()
    {
        var email = "user@example.com";
        var year = 2024;
        var jobId = "test-job-id";
        var errorStage = "Calculation";
        var errorMessage = string.Join(" ", Enumerable.Repeat("Error details with context", 100));

        var result = _service.GenerateErrorEmail(email, year, jobId, errorStage, errorMessage);

        Assert.NotNull(result.Subject);
        Assert.NotNull(result.HtmlBody);
        Assert.NotNull(result.TextBody);
        Assert.Contains(errorMessage, result.HtmlBody);
        Assert.Contains(errorMessage, result.TextBody);
    }

    [Fact]
    public void GenerateSuccessEmail_EmptyReportContent_HandlesCorrectly()
    {
        var email = "user@example.com";
        var year = 2024;
        var jobId = "test-job-id";
        var reportContent = string.Empty;

        var result = _service.GenerateSuccessEmail(email, year, jobId, reportContent);

        Assert.NotNull(result.Subject);
        Assert.NotNull(result.HtmlBody);
        Assert.NotNull(result.TextBody);
    }
}
