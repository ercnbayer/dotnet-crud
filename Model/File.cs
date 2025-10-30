namespace dotnetcrud.Model;

public class File
{
    public Guid Id { get; init; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public required string BucketName { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }

}