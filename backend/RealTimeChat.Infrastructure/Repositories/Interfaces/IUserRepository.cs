using RealTimeChat.Domain.Entities;

namespace RealTimeChat.Infrastructure.Repositories.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(string id);
    Task<AppUser?> GetByUsernameAsync(string username);
    Task<IEnumerable<AppUser>> GetAllAsync(int pageNumber = 1, int pageSize = 20);
    Task<IEnumerable<AppUser>> SearchAsync(string query);
    Task UpdateAsync(AppUser user);
    Task<int> SaveChangesAsync();
}
