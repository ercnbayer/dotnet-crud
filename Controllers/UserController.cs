using dotnetcrud.Dto;
using dotnetcrud.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace dotnetcrud.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost] // POST /user
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userDto)
        {
            var result = await _userService.CreateUserAsync(userDto);

            if (result is null)
                return StatusCode(500, new { error = "Null response" });

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error ?? "CreateUser" });

            return Ok(result.Data);
        }

        [HttpGet("{id}")] // GET /user/{id}
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
    }
}
