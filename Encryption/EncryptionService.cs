using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using dotnetcrud.Errors;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using static dotnetcrud.Security.Security;

namespace dotnetcrud.Encryption
{
    public class EncryptionService : IEncryptionService
    {
        private readonly SecuritySettings _settings;

        public EncryptionService(IOptions<SecuritySettings> options)
        {
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(_settings.AppKey)) throw new Exception("AppKey is not configured.");
            if (string.IsNullOrEmpty(_settings.Issuer)) throw new Exception("Issuer is not configured.");
            if (string.IsNullOrEmpty(_settings.Audience)) throw new Exception("Audience is not configured.");
            if (string.IsNullOrEmpty(_settings.Secret)) throw new Exception("JWT Secret is not configured.");
        }

        public (string hash, string salt) HashPassword(string password)
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }

            var salt = Convert.ToBase64String(saltBytes);
            var hash = ComputeHmac(password, salt);

            return (hash, salt);
        }

        public bool VerifyPassword(string password, string salt, string storedHash)
        {
            var computedHash = ComputeHmac(password, salt);
            return storedHash == computedHash;
        }

        private string ComputeHmac(string password, string salt)
        {
            var combined = $"{salt}{password}{_settings.AppKey}";

            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_settings.AppKey)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return Convert.ToBase64String(hashBytes);
            }
        }

        public string GenerateToken(Guid userId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userID", userId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        public Result<string> ValidateToken(string token) // Returns userID
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_settings.Secret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = true,
                ValidAudience = _settings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

                var userId = principal.FindFirst("userID")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Result<string>.Fail("Token does not contain userID.");

                return Result<string>.Success(userId);
            }
            catch (SecurityTokenExpiredException)
            {
                return Result<string>.Fail("Token expired.");
            }
            catch (Exception ex)
            {
                return Result<string>.Fail(ex.Message);
            }
        }

    }
}
