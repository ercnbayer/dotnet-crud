using dotnetcrud.Errors;
using dotnetcrud.Model;

namespace dotnetcrud.Repositories;

public interface IUserRepository
{
    Task<Result<IEnumerable<User>>> GetAllAsync();
    Task<Result<User>> GetByIdAsync(Guid id);
    Task<Result<User>> GetByEmailAsync(string email);
    Task<Result<User>> CreateAsync(User user);
    Task<Result<User>> UpdateAsync(User user);
    Task<Result<bool>> DeleteAsync(User id);
}