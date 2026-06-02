using AutoMapper;
using Microsoft.AspNetCore.Http;
using RealTimeChat.Application.DTOs.Users;
using RealTimeChat.Application.Interfaces;
using RealTimeChat.Infrastructure.Repositories.Interfaces;

namespace RealTimeChat.Application.Services.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AppUserDto>> GetAllAsync(int pageNumber = 1, int pageSize = 20)
    {
        var users = await _userRepository.GetAllAsync(pageNumber,pageSize);
        return _mapper.Map<IEnumerable<AppUserDto>>(users);
    }

    public async Task<AppUserDto?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("User id is required.", nameof(id));

        var user = await _userRepository.GetByIdAsync(id);

        if (user is null)
            return null;

        return _mapper.Map<AppUserDto>(user);
    }

    public async Task<AppUserDto?> GetByUsernameAsync(string userName)
    {
        var user = await _userRepository.GetByUsernameAsync(userName);

        if (user is null)
            return null;

        return _mapper.Map<AppUserDto>(user);
    }

    public async Task<IEnumerable<AppUserDto>> SearchAsync(string query)
    {
        var users = await _userRepository.SearchAsync(query);

        return _mapper.Map<IEnumerable<AppUserDto>>(users);
    }

    public async Task<AppUserDto?> UpdateUserAsync(string id, UpdateAppUserDto updatedUser)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            throw new InvalidOperationException("User not found.");

        user.FirstName = updatedUser.FirstName;
        user.LastName = updatedUser.LastName;

        if (updatedUser.ProfilePicture is not null && updatedUser.ProfilePicture.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/users");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePictureUrl.TrimStart('/'));

                if (File.Exists(oldPath))
                    File.Delete(oldPath);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(updatedUser.ProfilePicture.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await updatedUser.ProfilePicture.CopyToAsync(stream);
            }

            user.ProfilePictureUrl = $"/uploads/users/{fileName}";
        }

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return await GetByIdAsync(id);
    }
}
