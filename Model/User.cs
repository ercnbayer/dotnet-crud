
using System.ComponentModel.DataAnnotations;

namespace dotnetcrud.Model
{
    public class User
    {
        [Key]
        public Guid Id { get; init; }
  
        public required string Username { get; set; } 

        public required string Email { get; set; }

        public required string PasswordHash { get; set; }

        public required string Salt { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;

        // User ilişkisi
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}