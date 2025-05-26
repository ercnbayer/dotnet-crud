using System.Data.Common;
using dotnetcrud.Data;
using dotnetcrud.Errors;
using dotnetcrud.Model;
using Microsoft.EntityFrameworkCore;

namespace dotnetcrud.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<User>> CreateAsync(User user)
    {
        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Result<User>.Success(user);
        }
        catch (DbUpdateException ex)
        {
            return Result<User>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<User>.Fail(ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteAsync(User user)
    {
        try
        {
            _context.Users.Remove(user);
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

    public async Task<Result<IEnumerable<User>>> GetAllAsync()
    {
        try
        {
            var userResult = await _context.Users.ToListAsync();

            if (userResult.Count == 0)
            {
                return Result<IEnumerable<User>>.Fail("There are no users");
            }

            return Result<IEnumerable<User>>.Success(userResult);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<User>>.Fail(ex.Message);
        }
    }

    public async Task<Result<User>> GetByIdAsync(Guid id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return Result<User>.Fail("UserGetById Null Object Error");
            }

            return Result<User>.Success(user);
        }

        catch (Exception ex)
        {
            return Result<User>.Fail(ex.Message);
        }

    }

    public async Task<Result<User>> GetByEmailAsync(string email)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return Result<User>.Fail("User GetByEmail Null User Object");
            }
            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<User>.Fail(ex.Message);
        }
    }

    public async Task<Result<User>> UpdateAsync(User user)
    {
        try
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            try
            {

                var updatedUser = await _context.Users.FindAsync(user.Id);

                if (updatedUser == null)
                {
                    return Result<User>.Fail("Update User Null Error.");

                }

                return Result<User>.Success(updatedUser);
            }
            catch (Exception ex)
            {
                return Result<User>.Fail(ex.Message);
            }


        }

        catch (Exception ex)
        {
            return Result<User>.Fail(ex.Message);
        }
    }
}