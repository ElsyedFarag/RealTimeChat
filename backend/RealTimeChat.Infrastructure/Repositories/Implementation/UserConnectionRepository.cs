using Microsoft.EntityFrameworkCore;
using RealTimeChat.Domain.Entities;
using RealTimeChat.Infrastructure.Persistence;
using RealTimeChat.Infrastructure.Repositories.Interfaces;

namespace RealTimeChat.Infrastructure.Repositories.Implementation;

public class UserConnectionRepository : IUserConnectionRepository
{
    private readonly AppDbContext _context;

    public UserConnectionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(UserConnection connection)
    {
        await _context.UserConnections.AddAsync(connection);
    }

    public async Task RemoveAsync(string connectionId)
    {
        var connection = await _context.UserConnections
            .FirstOrDefaultAsync(x => x.ConnectionId == connectionId);

        if (connection is not null)
            _context.UserConnections.Remove(connection);
    }

    public async Task<int> GetConnectionCountAsync(string userId)
    {
        return await _context.UserConnections
            .CountAsync(x => x.UserId == userId);
    }

    public async Task<IEnumerable<string>> GetConnectionIdsByUserIdAsync(string userId)
    {
        return await _context.UserConnections
            .Where(x => x.UserId == userId)
            .Select(x => x.ConnectionId)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}