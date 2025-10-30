using dotnetcrud.Errors;
using dotnetcrud.Model;
using File = dotnetcrud.Model.File;

namespace dotnetcrud.Repositories;

public interface IFileRepository
{
    Task<Result<IEnumerable<File>>> GetAllAsync();
    Task<Result<File>> GetByIdAsync(Guid id);
    Task<Result<File>> CreateAsync(File file);
    Task<Result<File>> UpdateAsync(File file);
    Task<Result<bool>> DeleteAsync(File id);
}