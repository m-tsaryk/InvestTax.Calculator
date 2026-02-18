using Amazon.S3;
using Amazon.S3.Model;
using InvestTax.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace InvestTax.Infrastructure.AWS;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3Service> _logger;

    public S3Service(IAmazonS3 s3Client, ILogger<S3Service> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    public async Task<string> GetObjectAsStringAsync(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting object s3://{Bucket}/{Key}", bucketName, key);
        
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = key
        };

        using var response = await _s3Client.GetObjectAsync(request, cancellationToken);
        using var reader = new StreamReader(response.ResponseStream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    public async Task<Stream> GetObjectStreamAsync(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting object stream s3://{Bucket}/{Key}", bucketName, key);
        
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = key
        };

        var response = await _s3Client.GetObjectAsync(request, cancellationToken);
        return response.ResponseStream;
    }

    public async Task PutObjectAsync(string bucketName, string key, string content, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Putting object s3://{Bucket}/{Key}", bucketName, key);
        
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            ContentBody = content
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
    }

    public async Task PutObjectAsync(string bucketName, string key, Stream stream, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Putting object stream s3://{Bucket}/{Key}", bucketName, key);
        
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            InputStream = stream
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
    }

    public async Task<bool> ObjectExistsAsync(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = bucketName,
                Key = key
            };
            
            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task DeleteObjectAsync(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting object s3://{Bucket}/{Key}", bucketName, key);
        
        var request = new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = key
        };

        await _s3Client.DeleteObjectAsync(request, cancellationToken);
    }

    public async Task<Dictionary<string, string>> GetObjectMetadataAsync(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        var request = new GetObjectMetadataRequest
        {
            BucketName = bucketName,
            Key = key
        };

        var response = await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
        return response.Metadata.Keys.ToDictionary(k => k, k => response.Metadata[k]);
    }
}
