using Amazon.S3;

namespace dotnetcrud.S3;

public class S3Config
{
    public string ServiceUrl { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string BucketName { get; set; }
    public string QueueName { get; set; }
    public string Region { get; set; }
    
    public IAmazonS3 CreateS3Client()
    {
        var config = new AmazonS3Config
        {
            ServiceURL = ServiceUrl,
            ForcePathStyle = true // for localstack
        };

        return new AmazonS3Client(AccessKey, SecretKey, config);
    }

}