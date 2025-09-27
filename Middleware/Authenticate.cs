using dotnetcrud.Encryption;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace dotnetcrud.Middleware;

public class Authenticate : IAuthorizationFilter
{
    private readonly IEncryptionService _encryptionService;
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var authHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader))
        {
            context.Result = new JsonResult(new
            {
                Message = "Header is null"
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        var result = _encryptionService.ValidateToken(authHeader);

        if (!result.IsSuccess)
        {
            context.Result = new JsonResult(new
            {
                Message = result.Error
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }

        context.HttpContext.Items["UserId"] = result.Data;

    }
}