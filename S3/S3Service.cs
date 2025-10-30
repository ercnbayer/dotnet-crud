using System.Runtime.CompilerServices;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using dotnetcrud.Errors;

namespace dotnetcrud.S3;

public class S3Service : IS3Service
{

    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;


    public S3Service(S3Config config)
    {
        _s3Client = config.CreateS3Client();
        _bucketName = config.BucketName;


        EnsureBucketExists().Wait();
    }

    private async Task EnsureBucketExists()
    {
        var exists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName);
        if (!exists)
        {
            await _s3Client.PutBucketAsync(new PutBucketRequest
            {
                BucketName = _bucketName
            });
        }
    }

    public Result<string> CreatePresignedUploadUrl(string fileName, string contentType, Guid userId)
    {

        var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var key = $"{_bucketName}/{userId}/{fileName}-{unixTime}";



        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.Add(TimeSpan.FromDays(1)),
            Verb = HttpVerb.PUT
        };


        var url = _s3Client.GetPreSignedURL(request);
        if (url == null)
        {
            return Result<string>.Fail("Error creating presigned url");
        }

        Console.WriteLine(url);
        return Result<string>.Success(url);
    }


    public string DownloadFile(string key)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.Add(TimeSpan.FromDays(1)),
            Verb = HttpVerb.GET
        };

        var url = _s3Client.GetPreSignedURL(request);
        return url;
    }

    public async Task<List<string>> ListFilesAsync()
    {
        var response = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request
        {
            BucketName = _bucketName
        });

        return response.S3Objects.Select(o => o.Key).ToList();
    }
}
