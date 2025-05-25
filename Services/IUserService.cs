using dotnetcrud.Dto;
using dotnetcrud.Errors;
using dotnetcrud.Model;

namespace dotnetcrud.Services;

public interface IUserService
{
    Task<Result<IEnumerable<AuthenticatedUser>>> GetAllAsync();
    Task<Result<AuthenticatedUser>> GetUserByIdAsync(Guid id);
        
    Task<Result<AuthenticatedUser>> CreateUserAsync(CreateUserDto user);
    Task<Result<AuthenticatedUser>> UpdateUserAsync(Guid id,UpdateUserDto user);
    Task<Result<bool>> DeleteAsync(Guid id);
    Task<Result<User>> GetUserByEmailAsync(string email);

}