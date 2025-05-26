using System.ComponentModel.DataAnnotations;

namespace dotnetcrud.Dto;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [MinLength(8)]
    public required string Password;

}