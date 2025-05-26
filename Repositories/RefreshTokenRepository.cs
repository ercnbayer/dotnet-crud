using dotnetcrud.Data;
using dotnetcrud.Errors;
using dotnetcrud.Model;
using Microsoft.EntityFrameworkCore;

namespace dotnetcrud.Repositories;

public class RefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<RefreshToken>> CreateAsync(RefreshToken token)
    {

        try
        {
            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();
            return Result<RefreshToken>.Success(token);
        }
        catch (Exception e)
        {
            return Result<RefreshToken>.Fail("RefreshTokenRepository Create Error:" + e.Message);
        }


    }

    public async Task<Result<RefreshToken>> GetByIdAsync(Guid id)
    {

        try
        {
            var refreshToken = await _context.RefreshTokens.FindAsync(id);

            if (refreshToken == null)
            {
                return Result<RefreshToken>.Fail("RefreshTokenRepository GetById Error:" + id);
            }

            return Result<RefreshToken>.Success(refreshToken);
        }
        catch (Exception e)
        {

            return Result<RefreshToken>.Fail("RefreshTokenRepository GetById Error:" + e.Message);
        }

    }

    public async Task<Result<List<RefreshToken>>> GetByUserIdAsync(Guid userId)
    {

        try
        {
            var tokenList = await _context.RefreshTokens.Where(x => x.UserId == userId).ToListAsync();

            if (tokenList == null || tokenList.Count == 0)
            {
                return Result<List<RefreshToken>>.Fail("There are no tokens");
            }
            return Result<List<RefreshToken>>.Success(tokenList);
        }
        catch (Exception e)
        {
            return Result<List<RefreshToken>>.Fail("RefreshTokenRepository GetByUserId Error:" + e.Message);
        }


    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var token = await _context.RefreshTokens.FindAsync(id);
        if (token == null) return false;

        _context.RefreshTokens.Remove(token);
        await _context.SaveChangesAsync();
        return true;
    }
}