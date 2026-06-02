using RealTimeChat.Application.DTOs.Users;

namespace RealTimeChat.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<AppUserDto>> GetAllAsync(int pageNumber = 1, int pageSize = 20);
        Task<AppUserDto?> GetByIdAsync(string id);
        Task<AppUserDto?> GetByUsernameAsync(string userName);
        Task<AppUserDto?> UpdateUserAsync(string id, UpdateAppUserDto updatedUser);
        Task<IEnumerable<AppUserDto>> SearchAsync(string query);
    }
}
