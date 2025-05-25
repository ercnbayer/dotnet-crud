namespace dotnetcrud.Encryption;

public interface IEncryptionService
{
    public (string hash, string salt) HashPassword(string password);
    public bool VerifyPassword(string password, string salt, string storedHash);
    public IResult ValidateToken(string token);
    public string GenerateToken(Guid userId);

}