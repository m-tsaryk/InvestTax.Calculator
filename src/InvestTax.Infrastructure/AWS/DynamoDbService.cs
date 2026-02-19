using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using InvestTax.Core.Enums;
using InvestTax.Core.Interfaces;
using InvestTax.Core.Models;
using Microsoft.Extensions.Logging;

namespace InvestTax.Infrastructure.AWS;

public class DynamoDbService : IDynamoDbService
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly string _tableName;
    private readonly ILogger<DynamoDbService> _logger;

    public DynamoDbService(
        IAmazonDynamoDB dynamoDbClient,
        string tableName,
        ILogger<DynamoDbService> logger)
    {
        _dynamoDbClient = dynamoDbClient;
        _tableName = tableName;
        _logger = logger;
    }

    public async Task<Job?> GetJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting job {JobId} from DynamoDB", jobId);

        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "JobId", new AttributeValue { S = jobId } }
            }
        };

        var response = await _dynamoDbClient.GetItemAsync(request, cancellationToken);

        if (response.Item == null || response.Item.Count == 0)
        {
            return null;
        }

        return MapToJob(response.Item);
    }

    public async Task SaveJobAsync(Job job, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Saving job {JobId} to DynamoDB", job.JobId);

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = MapToAttributeValues(job)
        };

        await _dynamoDbClient.PutItemAsync(request, cancellationToken);
    }

    public async Task UpdateJobStatusAsync(
        string jobId,
        JobStatus status,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating job {JobId} status to {Status}", jobId, status);

        var updateExpression = "SET #status = :status, UpdatedAt = :updatedAt";
        var expressionAttributeNames = new Dictionary<string, string>
        {
            { "#status", "Status" }
        };
        var expressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            { ":status", new AttributeValue { S = status.ToString() } },
            { ":updatedAt", new AttributeValue { S = DateTime.UtcNow.ToString("o") } }
        };

        if (status == JobStatus.Completed)
        {
            updateExpression += ", CompletedAt = :completedAt";
            expressionAttributeValues[":completedAt"] = new AttributeValue { S = DateTime.UtcNow.ToString("o") };
        }

        if (!string.IsNullOrEmpty(errorMessage))
        {
            updateExpression += ", ErrorMessage = :errorMessage";
            expressionAttributeValues[":errorMessage"] = new AttributeValue { S = errorMessage };
        }

        var request = new UpdateItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "JobId", new AttributeValue { S = jobId } }
            },
            UpdateExpression = updateExpression,
            ExpressionAttributeNames = expressionAttributeNames,
            ExpressionAttributeValues = expressionAttributeValues
        };

        await _dynamoDbClient.UpdateItemAsync(request, cancellationToken);
    }

    public async Task<List<Job>> GetJobsByStatusAsync(
        JobStatus status,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Querying jobs by status {Status}", status);

        var request = new ScanRequest
        {
            TableName = _tableName,
            FilterExpression = "#status = :status",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#status", "Status" }
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":status", new AttributeValue { S = status.ToString() } }
            },
            Limit = limit
        };

        var response = await _dynamoDbClient.ScanAsync(request, cancellationToken);
        return response.Items.Select(MapToJob).ToList();
    }

    private Job MapToJob(Dictionary<string, AttributeValue> item)
    {
        return new Job
        {
            JobId = item["JobId"].S,
            Email = item["Email"].S,
            S3Key = item["S3Key"].S,
            Status = Enum.Parse<JobStatus>(item["Status"].S),
            CreatedAt = item["CreatedAt"].S,
            UpdatedAt = item["UpdatedAt"].S,
            CompletedAt = item.ContainsKey("CompletedAt") ? item["CompletedAt"].S : null,
            ErrorMessage = item.ContainsKey("ErrorMessage") ? item["ErrorMessage"].S : null,
            ExecutionArn = item.ContainsKey("ExecutionArn") ? item["ExecutionArn"].S : null,
            TransactionCount = item.ContainsKey("TransactionCount") ? int.Parse(item["TransactionCount"].N) : null,
            DurationSeconds = item.ContainsKey("DurationSeconds") ? double.Parse(item["DurationSeconds"].N) : null
        };
    }

    private Dictionary<string, AttributeValue> MapToAttributeValues(Job job)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "JobId", new AttributeValue { S = job.JobId } },
            { "Email", new AttributeValue { S = job.Email } },
            { "S3Key", new AttributeValue { S = job.S3Key } },
            { "Status", new AttributeValue { S = job.Status.ToString() } },
            { "CreatedAt", new AttributeValue { S = job.CreatedAt } },
            { "UpdatedAt", new AttributeValue { S = job.UpdatedAt } }
        };

        if (!string.IsNullOrEmpty(job.CompletedAt))
            item["CompletedAt"] = new AttributeValue { S = job.CompletedAt };
        
        if (!string.IsNullOrEmpty(job.ErrorMessage))
            item["ErrorMessage"] = new AttributeValue { S = job.ErrorMessage };
        
        if (!string.IsNullOrEmpty(job.ExecutionArn))
            item["ExecutionArn"] = new AttributeValue { S = job.ExecutionArn };
        
        if (job.TransactionCount.HasValue)
            item["TransactionCount"] = new AttributeValue { N = job.TransactionCount.Value.ToString() };
        
        if (job.DurationSeconds.HasValue)
            item["DurationSeconds"] = new AttributeValue { N = job.DurationSeconds.Value.ToString() };

        return item;
    }
}
