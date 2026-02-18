using InvestTax.Core.Enums;
using InvestTax.Core.Models;

namespace InvestTax.Core.Interfaces;

/// <summary>
/// Abstraction for DynamoDB operations
/// </summary>
public interface IDynamoDbService
{
    Task<Job?> GetJobAsync(string jobId, CancellationToken cancellationToken = default);
    Task SaveJobAsync(Job job, CancellationToken cancellationToken = default);
    Task UpdateJobStatusAsync(string jobId, JobStatus status, string? errorMessage = null, CancellationToken cancellationToken = default);
    Task<List<Job>> GetJobsByStatusAsync(JobStatus status, int limit = 100, CancellationToken cancellationToken = default);
}
