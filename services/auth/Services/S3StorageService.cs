using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Auth.Api.Configuration;
using Microsoft.Extensions.Options;

namespace Auth.Api.Services;

public interface IStorageService
{
    Task EnsureBucketAsync(string bucketName, CancellationToken cancellationToken = default);
    Task<string> UploadFileAsync(string bucketName, string fileName, Stream fileStream, string contentType, CancellationToken cancellationToken = default);
    Task<(Stream Stream, string ContentType)> GetFileAsync(string bucketName, string fileName, CancellationToken cancellationToken = default);
}

public class S3StorageService(IAmazonS3 s3Client, IOptions<StorageOptions> options) : IStorageService
{
    private readonly StorageOptions _options = options.Value;

    public async Task EnsureBucketAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        if (await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName))
            return;

        await s3Client.PutBucketAsync(new PutBucketRequest { BucketName = bucketName }, cancellationToken);
    }

    public async Task<string> UploadFileAsync(
        string bucketName,
        string fileName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = fileName,
            InputStream = fileStream,
            ContentType = contentType
        };

        var response = await s3Client.PutObjectAsync(putRequest, cancellationToken);

        if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            return fileName;

        throw new InvalidOperationException("Falha ao salvar o arquivo no Object Storage.");
    }

    public async Task<(Stream Stream, string ContentType)> GetFileAsync(
        string bucketName,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var s3Object = await s3Client.GetObjectAsync(bucketName, fileName, cancellationToken);
        return (s3Object.ResponseStream, s3Object.Headers.ContentType);
    }
}
