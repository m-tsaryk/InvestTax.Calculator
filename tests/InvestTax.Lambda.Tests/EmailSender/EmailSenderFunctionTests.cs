using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using InvestTax.Lambda.EmailSender;
using InvestTax.Lambda.EmailSender.Models;
using InvestTax.Lambda.EmailSender.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace InvestTax.Lambda.Tests.EmailSender;

public class EmailSenderFunctionTests
{
    private readonly Mock<IAmazonS3> _s3Mock;
    private readonly Mock<IAmazonSimpleEmailService> _sesMock;
    private readonly Mock<IEmailTemplateService> _templateServiceMock;
    private readonly Mock<ILogger<Function>> _loggerMock;
    private readonly Mock<ILambdaContext> _contextMock;
    private readonly Function _function;

    public EmailSenderFunctionTests()
    {
        _s3Mock = new Mock<IAmazonS3>();
        _sesMock = new Mock<IAmazonSimpleEmailService>();
        _templateServiceMock = new Mock<IEmailTemplateService>();
        _loggerMock = new Mock<ILogger<Function>>();
        _contextMock = new Mock<ILambdaContext>();
        
        _contextMock.Setup(c => c.RemainingTime).Returns(TimeSpan.FromMinutes(5));
        
        _function = new Function(_s3Mock.Object, _sesMock.Object, _templateServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task FunctionHandler_SuccessEmail_LoadsReportAndSendsEmail()
    {
        var input = new EmailInput
        {
            JobId = "test-job-123",
            Email = "user@example.com",
            Year = 2024,
            ProcessingBucket = "test-bucket",
            TextReportKey = "reports/test-job-123.txt",
            IsSuccess = true
        };

        var reportContent = "Sample tax report content";
        var messageId = "ses-message-123";

        SetupS3GetObject(input.ProcessingBucket, input.TextReportKey, reportContent);
        SetupTemplateService(true, input.Email, input.Year, input.JobId, reportContent);
        SetupSesSendEmail(messageId);

        var result = await _function.FunctionHandler(input, _contextMock.Object);

        Assert.True(result.Success);
        Assert.Equal(input.JobId, result.JobId);
        Assert.Equal(messageId, result.MessageId);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task FunctionHandler_ErrorEmail_SendsErrorEmailWithoutLoadingReport()
    {
        var input = new EmailInput
        {
            JobId = "test-job-456",
            Email = "user@example.com",
            Year = 2024,
            ProcessingBucket = "test-bucket",
            IsSuccess = false,
            ErrorStage = "Validation",
            ErrorMessage = "CSV validation failed"
        };

        var messageId = "ses-message-456";

        SetupTemplateService(false, input.Email, input.Year, input.JobId, errorStage: input.ErrorStage, errorMessage: input.ErrorMessage);
        SetupSesSendEmail(messageId);

        var result = await _function.FunctionHandler(input, _contextMock.Object);

        Assert.True(result.Success);
        Assert.Equal(input.JobId, result.JobId);
        Assert.Equal(messageId, result.MessageId);
        _s3Mock.Verify(s => s.GetObjectAsync(It.IsAny<GetObjectRequest>(), default), Times.Never);
    }

    [Fact]
    public async Task FunctionHandler_SuccessEmailWithoutReportKey_ReturnsError()
    {
        var input = new EmailInput
        {
            JobId = "test-job-789",
            Email = "user@example.com",
            Year = 2024,
            ProcessingBucket = "test-bucket",
            TextReportKey = null,
            IsSuccess = true
        };

        var result = await _function.FunctionHandler(input, _contextMock.Object);

        Assert.False(result.Success);
        Assert.Equal(input.JobId, result.JobId);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("TextReportKey is required", result.ErrorMessage);
    }

    [Fact]
    public async Task FunctionHandler_S3LoadFails_ReturnsError()
    {
        var input = new EmailInput
        {
            JobId = "test-job-error",
            Email = "user@example.com",
            Year = 2024,
            ProcessingBucket = "test-bucket",
            TextReportKey = "reports/missing.txt",
            IsSuccess = true
        };

        _s3Mock.Setup(s => s.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
            .ThrowsAsync(new AmazonS3Exception("Object not found"));

        var result = await _function.FunctionHandler(input, _contextMock.Object);

        Assert.False(result.Success);
        Assert.Equal(input.JobId, result.JobId);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("Email sending failed", result.ErrorMessage);
    }

    [Fact]
    public async Task FunctionHandler_SesSendFails_ReturnsError()
    {
        var input = new EmailInput
        {
            JobId = "test-job-ses-error",
            Email = "user@example.com",
            Year = 2024,
            ProcessingBucket = "test-bucket",
            TextReportKey = "reports/test.txt",
            IsSuccess = true
        };

        var reportContent = "Sample report";
        SetupS3GetObject(input.ProcessingBucket, input.TextReportKey, reportContent);
        SetupTemplateService(true, input.Email, input.Year, input.JobId, reportContent);

        _sesMock.Setup(s => s.SendEmailAsync(It.IsAny<SendEmailRequest>(), default))
            .ThrowsAsync(new Exception("SES service unavailable"));

        var result = await _function.FunctionHandler(input, _contextMock.Object);

        Assert.False(result.Success);
        Assert.Equal(input.JobId, result.JobId);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("Email sending failed", result.ErrorMessage);
    }

    [Fact]
    public async Task FunctionHandler_SuccessEmail_CallsS3WithCorrectParameters()
    {
        var input = new EmailInput
        {
            JobId = "test-job",
            Email = "user@example.com",
            Year = 2024,
            ProcessingBucket = "my-bucket",
            TextReportKey = "reports/my-report.txt",
            IsSuccess = true
        };

        var reportContent = "Report content";
        SetupS3GetObject(input.ProcessingBucket, input.TextReportKey, reportContent);
        SetupTemplateService(true, input.Email, input.Year, input.JobId, reportContent);
        SetupSesSendEmail("msg-id");

        await _function.FunctionHandler(input, _contextMock.Object);

        _s3Mock.Verify(s => s.GetObjectAsync(
            It.Is<GetObjectRequest>(r => 
                r.BucketName == input.ProcessingBucket && 
                r.Key == input.TextReportKey),
            default), Times.Once);
    }

    [Fact]
    public async Task FunctionHandler_SuccessEmail_CallsSesWithCorrectRecipient()
    {
        var input = new EmailInput
        {
            JobId = "test-job",
            Email = "recipient@example.com",
            Year = 2024,
            ProcessingBucket = "bucket",
            TextReportKey = "reports/report.txt",
            IsSuccess = true
        };

        var reportContent = "Report";
        SetupS3GetObject(input.ProcessingBucket, input.TextReportKey, reportContent);
        SetupTemplateService(true, input.Email, input.Year, input.JobId, reportContent);
        SetupSesSendEmail("msg-id");

        await _function.FunctionHandler(input, _contextMock.Object);

        _sesMock.Verify(s => s.SendEmailAsync(
            It.Is<SendEmailRequest>(r => 
                r.Destination.ToAddresses.Contains(input.Email)),
            default), Times.Once);
    }

    [Fact]
    public async Task FunctionHandler_SuccessEmail_SendsBothHtmlAndTextVersions()
    {
        var input = new EmailInput
        {
            JobId = "test-job",
            Email = "user@example.com",
            Year = 2024,
            ProcessingBucket = "bucket",
            TextReportKey = "reports/report.txt",
            IsSuccess = true
        };

        var reportContent = "Report";
        SetupS3GetObject(input.ProcessingBucket, input.TextReportKey, reportContent);
        SetupTemplateService(true, input.Email, input.Year, input.JobId, reportContent);
        SetupSesSendEmail("msg-id");

        await _function.FunctionHandler(input, _contextMock.Object);

        _sesMock.Verify(s => s.SendEmailAsync(
            It.Is<SendEmailRequest>(r => 
                r.Message.Body.Html != null && 
                r.Message.Body.Text != null),
            default), Times.Once);
    }

    [Fact]
    public async Task FunctionHandler_ErrorEmail_CallsTemplateServiceWithErrorDetails()
    {
        var input = new EmailInput
        {
            JobId = "error-job",
            Email = "user@example.com",
            Year = 2024,
            ProcessingBucket = "bucket",
            IsSuccess = false,
            ErrorStage = "NBP Rate Fetching",
            ErrorMessage = "Rate not found for 2024-01-15"
        };

        SetupTemplateService(false, input.Email, input.Year, input.JobId, 
            errorStage: input.ErrorStage, errorMessage: input.ErrorMessage);
        SetupSesSendEmail("msg-id");

        await _function.FunctionHandler(input, _contextMock.Object);

        _templateServiceMock.Verify(t => t.GenerateErrorEmail(
            input.Email,
            input.Year,
            input.JobId,
            input.ErrorStage,
            input.ErrorMessage), Times.Once);
    }

    [Fact]
    public async Task FunctionHandler_ErrorEmailWithNullErrorMessage_UsesDefaultMessage()
    {
        var input = new EmailInput
        {
            JobId = "error-job",
            Email = "user@example.com",
            Year = 2024,
            ProcessingBucket = "bucket",
            IsSuccess = false,
            ErrorStage = null,
            ErrorMessage = null
        };

        SetupTemplateService(false, input.Email, input.Year, input.JobId, 
            errorStage: "Unknown", errorMessage: It.IsAny<string>());
        SetupSesSendEmail("msg-id");

        await _function.FunctionHandler(input, _contextMock.Object);

        _templateServiceMock.Verify(t => t.GenerateErrorEmail(
            input.Email,
            input.Year,
            input.JobId,
            "Unknown",
            It.Is<string>(s => s.Contains("unknown error"))), Times.Once);
    }

    [Fact]
    public async Task FunctionHandler_NullInput_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _function.FunctionHandler(null!, _contextMock.Object));
    }

    private void SetupS3GetObject(string bucket, string key, string content)
    {
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var response = new GetObjectResponse
        {
            ResponseStream = stream,
            HttpStatusCode = HttpStatusCode.OK
        };

        _s3Mock.Setup(s => s.GetObjectAsync(
            It.Is<GetObjectRequest>(r => r.BucketName == bucket && r.Key == key),
            default))
            .ReturnsAsync(response);
    }

    private void SetupTemplateService(bool isSuccess, string email, int year, string jobId, 
        string reportContent = "", string errorStage = "", string errorMessage = "")
    {
        if (isSuccess)
        {
            _templateServiceMock.Setup(t => t.GenerateSuccessEmail(email, year, jobId, reportContent))
                .Returns(new EmailContent
                {
                    Subject = "Test Subject",
                    HtmlBody = "<html>Test HTML</html>",
                    TextBody = "Test Text"
                });
        }
        else
        {
            _templateServiceMock.Setup(t => t.GenerateErrorEmail(email, year, jobId, 
                It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new EmailContent
                {
                    Subject = "Error Subject",
                    HtmlBody = "<html>Error HTML</html>",
                    TextBody = "Error Text"
                });
        }
    }

    private void SetupSesSendEmail(string messageId)
    {
        var response = new SendEmailResponse
        {
            MessageId = messageId,
            HttpStatusCode = HttpStatusCode.OK
        };

        _sesMock.Setup(s => s.SendEmailAsync(It.IsAny<SendEmailRequest>(), default))
            .ReturnsAsync(response);
    }
}
