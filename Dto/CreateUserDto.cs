using System.ComponentModel.DataAnnotations;

namespace dotnetcrud.Dto;

public class CreateUserDto
{
    [Required]
    [StringLength(100)]
    [MinLength(3)]
    public required string Username { get; set; }


    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [MinLength(8)]
    public required string Password { get; set; }
}