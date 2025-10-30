using Amazon.S3;
using Amazon.S3.Model;
using dotnetcrud.Errors;
using dotnetcrud.Repositories;
using dotnetcrud.S3;

namespace dotnetcrud.Services;

public class FileService
{

    private readonly IFileRepository _fileRepository;
    public FileService(IAmazonS3 s3, S3Config config, IFileRepository fileRepository)
    {
        _fileRepository = fileRepository;
    }
    
}