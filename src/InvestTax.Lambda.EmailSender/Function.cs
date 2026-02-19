using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using InvestTax.Lambda.EmailSender.Models;
using InvestTax.Lambda.EmailSender.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace InvestTax.Lambda.EmailSender;

/// <summary>
/// Lambda function to send tax calculation reports via email
/// </summary>
public class Function
{
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonSimpleEmailService _sesClient;
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<Function> _logger;
    private readonly string _fromEmail;

    /// <summary>
    /// Default constructor used by Lambda runtime
    /// </summary>
    public Function() : this(
        new AmazonS3Client(),
        new AmazonSimpleEmailServiceClient(),
        new EmailTemplateService(),
        CreateServiceProvider().GetRequiredService<ILogger<Function>>())
    {
    }

    /// <summary>
    /// Constructor for dependency injection (testing)
    /// </summary>
    public Function(
        IAmazonS3 s3Client,
        IAmazonSimpleEmailService sesClient,
        IEmailTemplateService templateService,
        ILogger<Function> logger)
    {
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        _sesClient = sesClient ?? throw new ArgumentNullException(nameof(sesClient));
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Get from email from environment variable or use default
        _fromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL") ?? "noreply@investtax.example.com";
    }

    /// <summary>
    /// Lambda function handler for email sending
    /// </summary>
    /// <param name="input">Email input with report details or error information</param>
    /// <param name="context">Lambda context</param>
    /// <returns>Email output with SES message ID</returns>
    public async Task<EmailOutput> FunctionHandler(EmailInput input, ILambdaContext context)
    {
        ArgumentNullException.ThrowIfNull(input);
        
        _logger.LogInformation("Starting email send for Job ID: {JobId}, IsSuccess: {IsSuccess}", 
            input.JobId, input.IsSuccess);

        try
        {
            EmailContent emailContent;

            if (input.IsSuccess)
            {
                // Success email - load report and generate success email
                _logger.LogInformation("Generating success email for Job ID: {JobId}", input.JobId);
                
                if (string.IsNullOrWhiteSpace(input.TextReportKey))
                {
                    throw new InvalidOperationException("TextReportKey is required for success emails");
                }

                var reportContent = await LoadTextReportAsync(
                    input.ProcessingBucket, 
                    input.TextReportKey, 
                    context.RemainingTime);

                emailContent = _templateService.GenerateSuccessEmail(
                    input.Email,
                    input.Year,
                    input.JobId,
                    reportContent);
            }
            else
            {
                // Error email - generate error email
                _logger.LogInformation("Generating error email for Job ID: {JobId}, Stage: {Stage}", 
                    input.JobId, input.ErrorStage ?? "Unknown");

                var errorMessage = input.ErrorMessage ?? "An unknown error occurred during processing";
                var errorStage = input.ErrorStage ?? "Unknown";

                emailContent = _templateService.GenerateErrorEmail(
                    input.Email,
                    input.Year,
                    input.JobId,
                    errorStage,
                    errorMessage);
            }

            // Send email via SES
            _logger.LogInformation("Sending email to {Email} from {From}", input.Email, _fromEmail);

            var messageId = await SendEmailAsync(
                input.Email,
                emailContent.Subject,
                emailContent.HtmlBody,
                emailContent.TextBody);

            _logger.LogInformation("Email sent successfully. MessageId: {MessageId}", messageId);

            return new EmailOutput
            {
                JobId = input.JobId,
                Success = true,
                MessageId = messageId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email sending failed for Job ID: {JobId}", input.JobId);
            
            return new EmailOutput
            {
                JobId = input.JobId,
                Success = false,
                ErrorMessage = $"Email sending failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Load text report from S3
    /// </summary>
    private async Task<string> LoadTextReportAsync(
        string bucket, 
        string key, 
        TimeSpan remainingTime)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = bucket,
            Key = key
        };

        using var response = await _s3Client.GetObjectAsync(getRequest);
        using var reader = new StreamReader(response.ResponseStream);
        var content = await reader.ReadToEndAsync();

        _logger.LogInformation("Loaded text report: {Length} characters", content.Length);

        return content;
    }

    /// <summary>
    /// Send email via Amazon SES
    /// </summary>
    private async Task<string> SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string textBody)
    {
        var sendRequest = new SendEmailRequest
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
                    Html = new Content
                    {
                        Charset = "UTF-8",
                        Data = htmlBody
                    },
                    Text = new Content
                    {
                        Charset = "UTF-8",
                        Data = textBody
                    }
                }
            }
        };

        var response = await _sesClient.SendEmailAsync(sendRequest);

        return response.MessageId;
    }

    /// <summary>
    /// Create service provider for dependency injection
    /// </summary>
    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
        });
        
        return services.BuildServiceProvider();
    }
}

