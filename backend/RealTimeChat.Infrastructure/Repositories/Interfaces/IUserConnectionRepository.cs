using RealTimeChat.Domain.Entities;

namespace RealTimeChat.Infrastructure.Repositories.Interfaces;

public interface IUserConnectionRepository
{
    Task AddAsync(UserConnection connection);

    Task RemoveAsync(string connectionId);

    Task<int> GetConnectionCountAsync(string userId);

    Task<IEnumerable<string>> GetConnectionIdsByUserIdAsync(string userId);

    Task SaveChangesAsync();
}