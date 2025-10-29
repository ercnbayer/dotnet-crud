using dotnetcrud.Dto;
using dotnetcrud.Services;
using Microsoft.AspNetCore.Mvc;

using dotnetcrud.Middleware;
using dotnetcrud.S3;


namespace dotnetcrud.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        //private readonly IS3Service _Is3Service;

        public UserController(IUserService userService)
        {
            _userService = userService;
            //_Is3Service = is3Service;
        }

        [HttpPost] // POST /user
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userDto)
        {
            var result = await _userService.CreateUserAsync(userDto);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error ?? "CreateUser" });

            return Ok(result.Data);
        }

        [HttpGet("{id}")] // GET /user/{id}
        [ServiceFilter(typeof(Authenticate))]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var result = await _userService.GetUserByIdAsync(id);

            if (!result.IsSuccess)
                return NotFound(new { error = result.Error ?? "User not found" });

            return Ok(result.Data);
        }

        [HttpDelete("{id}")] // DELETE /user/{id}
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var result = await _userService.DeleteAsync(id);

            if (!result.IsSuccess)
                return NotFound(new { error = result.Error ?? "User can't be deleted" });

            return Ok(result.Data);
        }

        [HttpPatch("{id}")] // PATCH /user/{id}
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto updateUserDto)
        {
            var result = await _userService.UpdateUserAsync(id, updateUserDto);

            if (!result.IsSuccess)
                return NotFound(new { error = result.Error ?? "User can't be updated" });

            return Ok(result.Data);
        }

        //[ServiceFilter(typeof(Authenticate))]
        //[HttpPost("uploadFile")]
        /*public async Task<IActionResult> UploadFile([FromBody] S3Upload fileDto)
        {
            var userId = HttpContext.Items["UserId"];
            if (userId == null)
            {
                return Problem("null user ", null, 401);
            }


            if (!Guid.TryParse(userId.ToString(), out var id))
            {
                return Problem("Invalid User ID format.", null, 400);
            }

            var urlResult = _Is3Service.CreatePresignedUploadUrl(fileDto.FileName, fileDto.ContentType, id);

            if (!urlResult.IsSuccess)
            {
                return Problem(urlResult.Error, null, 400);
            }

            return Ok(urlResult.Data);*/

        }
    }
}