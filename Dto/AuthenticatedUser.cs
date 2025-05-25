using dotnetcrud.Model;

namespace dotnetcrud.Dto;

public class AuthenticatedUser
{
    public required Guid Id { get; init; }
    public required string Username { get; set; }

    public required string Email { get; set; }

    
}