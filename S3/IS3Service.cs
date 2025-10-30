using dotnetcrud.Errors;

namespace dotnetcrud.S3;

public interface IS3Service
{
    public Result<string> CreatePresignedUploadUrl(string fileName, string contentType, Guid userId);

}