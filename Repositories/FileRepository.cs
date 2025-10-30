
using System.Data.Common;
using dotnetcrud.Data;
using dotnetcrud.Errors;
using Microsoft.EntityFrameworkCore;
using dotnetcrud.Model;
using File = dotnetcrud.Model.File;

namespace dotnetcrud.Repositories;

public class FileRepository : IFileRepository
{
    private readonly AppDbContext _context;

    public FileRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<Result<File>> CreateAsync(File file)
    {
        try
        {
            _context.Files.Add(file);
            await _context.SaveChangesAsync();
            return Result<File>.Success(file);
        }
        catch (DbUpdateException ex)
        {

            return Result<File>.Fail(ex.Message);// to do get detailed err
        }
        catch (Exception ex)
        {

            return Result<File>.Fail(ex.Message);
        }
    }

    public async Task<Result<File>> UpdateAsync(File file)
    {
        try
        {
            _context.Files.Update(file);
            await _context.SaveChangesAsync();
            try
            {

                var updatedFile = await _context.Files.FindAsync(file.Id);

                if (updatedFile == null)
                {
                    return Result<File>.Fail("Update File Null Error.");

                }

                return Result<File>.Success(updatedFile);
            }
            catch (Exception ex)
            {
                return Result<File>.Fail(ex.Message);
            }
        }
        catch (DbUpdateException ex)
        {
            return Result<File>.Fail(ex.Message);
        }
    }


    public async Task<Result<bool>> DeleteAsync(File file)
    {
        try
        {
            _context.Files.Remove(file);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
        catch (DbException dbex)
        {
            return Result<bool>.Fail(dbex.Message);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }

    public async Task<Result<IEnumerable<File>>> GetAllAsync()
    {
        try
        {
            var fileResult = await _context.Files.ToListAsync();

            if (fileResult.Count == 0)
            {
                return Result<IEnumerable<File>>.Fail("There are no Files");
            }

            return Result<IEnumerable<File>>.Success(fileResult);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<File>>.Fail(ex.Message);
        }
    }

    public async Task<Result<File>> GetByIdAsync(Guid id)
    {
        try
        {
            var file = await _context.Files.FindAsync(id);

            if (file == null)
            {
                return Result<File>.Fail("FileGetById Null Object Error");
            }

            return Result<File>.Success(file);
        }

        catch (Exception ex)
        {
            return Result<File>.Fail(ex.Message);
        }

    }

}
