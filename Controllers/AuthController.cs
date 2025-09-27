using dotnetcrud.Dto;
using dotnetcrud.Encryption;
using dotnetcrud.Model;
using dotnetcrud.Services;
using Microsoft.AspNetCore.Mvc;

namespace dotnetcrud.Controllers;
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IEncryptionService _encryptionService;

    public AuthController(IUserService userService, IEncryptionService encryptionService)
    {
        _userService = userService;
        _encryptionService = encryptionService;
    }
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginDto req)
    {
        var userResult = await _userService.GetUserByEmailAsync(req.Email);

        if (!userResult.IsSuccess)
            return BadRequest(new { error = userResult.Error ?? "User Login" });

        if (!_encryptionService.VerifyPassword(
                req.Password,
                userResult.Data.Salt,
                userResult.Data.PasswordHash
            ))
        {
            return Unauthorized();
        }


        var tokenString = _encryptionService.GenerateToken(userResult.Data.Id);


        var authenticatedUser = new AuthenticatedUser
        {
            Id = userResult.Data.Id,
            Email = userResult.Data.Email,
            Username = userResult.Data.Username,
        };


        var response = new LoginResponse
        {
            User = authenticatedUser,
            AccessToken = tokenString,
            /* To Do: Add Here RefreshToken*/
        };
        return Ok(response);

    }

    // TO DO: Delete UserCreate from UserController Add it here as Register

}