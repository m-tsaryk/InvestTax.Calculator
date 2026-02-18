namespace InvestTax.Core.Interfaces;

/// <summary>
/// Abstraction for S3 operations
/// </summary>
public interface IS3Service
{
    Task<string> GetObjectAsStringAsync(string bucketName, string key, CancellationToken cancellationToken = default);
    Task<Stream> GetObjectStreamAsync(string bucketName, string key, CancellationToken cancellationToken = default);
    Task PutObjectAsync(string bucketName, string key, string content, CancellationToken cancellationToken = default);
    Task PutObjectAsync(string bucketName, string key, Stream stream, CancellationToken cancellationToken = default);
    Task<bool> ObjectExistsAsync(string bucketName, string key, CancellationToken cancellationToken = default);
    Task DeleteObjectAsync(string bucketName, string key, CancellationToken cancellationToken = default);
    Task<Dictionary<string, string>> GetObjectMetadataAsync(string bucketName, string key, CancellationToken cancellationToken = default);
}
