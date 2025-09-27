using dotnetcrud.Dto;
using dotnetcrud.Encryption;
using dotnetcrud.Errors;
using dotnetcrud.Model;
using dotnetcrud.Repositories;

namespace dotnetcrud.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEncryptionService _encryptionService;

    public UserService(IUserRepository userRepository, IEncryptionService encryptionService)
    {
        _userRepository = userRepository;
        _encryptionService = encryptionService;
    }

    public async Task<Result<IEnumerable<AuthenticatedUser>>> GetAllAsync()
    {
        var userResults = await _userRepository.GetAllAsync();

        if (!userResults.IsSuccess)
        {
            return Result<IEnumerable<AuthenticatedUser>>.Fail(userResults.Error);
        }

        var authenticatedUsers = userResults.Data.Select(user => new AuthenticatedUser
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,

        });



        return Result<IEnumerable<AuthenticatedUser>>.Success(authenticatedUsers);
    }


    public async Task<Result<AuthenticatedUser>> GetUserByIdAsync(Guid id)
    {
        var dbUserResult = await _userRepository.GetByIdAsync(id);

        if (dbUserResult.IsSuccess == false)
        {
            return Result<AuthenticatedUser>.Fail(dbUserResult.Error);

        }

        var user = new AuthenticatedUser
        {
            Id = dbUserResult.Data.Id,
            Username = dbUserResult.Data.Username,
            Email = dbUserResult.Data.Email,
        };




        return Result<AuthenticatedUser>.Success(user);

    }

    public async Task<Result<User>> GetUserByEmailAsync(string email)
    {
        var dbUserResult = await _userRepository.GetByEmailAsync(email);

        if (!dbUserResult.IsSuccess)
        {
            return Result<User>.Fail(dbUserResult.Error);

        }


        return Result<User>.Success(dbUserResult.Data);

    }

    public async Task<Result<AuthenticatedUser>> CreateUserAsync(CreateUserDto dto)
    {
        var (passwordHash, salt) = _encryptionService.HashPassword(dto.Password);
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = passwordHash,
            Salt = salt
        };


        var dbUserResult = await _userRepository.CreateAsync(user);

        if (!dbUserResult.IsSuccess)
        {
            if (dbUserResult != null)
                return Result<AuthenticatedUser>.Fail(dbUserResult.Error);

        }

        var authenticatedUser = new AuthenticatedUser
        {
            Id = dbUserResult.Data.Id,
            Username = dbUserResult.Data.Username,
            Email = dbUserResult.Data.Email,
        };

        return Result<AuthenticatedUser>.Success(authenticatedUser);
    }




    public async Task<Result<AuthenticatedUser>> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        var userResult = await _userRepository.GetByIdAsync(id);
        if (userResult.IsSuccess == false)
            return Result<AuthenticatedUser>.Fail(userResult.Error);


        var userUpdateResult = await _userRepository.UpdateAsync(userResult.Data);

        if (userUpdateResult.IsSuccess == false)
            return Result<AuthenticatedUser>.Fail(userUpdateResult.Error);

        var authenticatedUser = new AuthenticatedUser
        {
            Id = userUpdateResult.Data.Id,
            Username = userUpdateResult.Data.Username,
            Email = userUpdateResult.Data.Email,
        };


        return Result<AuthenticatedUser>.Success(authenticatedUser);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var userResult = await _userRepository.GetByIdAsync(id);

        if (userResult.IsSuccess == false)
            return Result<bool>.Fail(userResult.Error);

        var isDeleted = await _userRepository.DeleteAsync(userResult.Data);

        if (isDeleted.IsSuccess == false)
            return Result<bool>.Fail(isDeleted.Error);

        return Result<bool>.Success(isDeleted.Data);
    }
}