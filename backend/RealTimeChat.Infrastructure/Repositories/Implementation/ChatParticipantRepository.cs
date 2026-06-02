using Microsoft.EntityFrameworkCore;
using RealTimeChat.Domain.Entities;
using RealTimeChat.Infrastructure.Persistence;
using RealTimeChat.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;

namespace RealTimeChat.Infrastructure.Repositories.Implementation;

public class ChatParticipantRepository : IChatParticipantRepository
{
    private readonly AppDbContext _appDbContext;

    public ChatParticipantRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    
    public async Task AddAsync(ChatParticipant chatParticipant)
    {
        await _appDbContext.ChatParticipants.AddAsync(chatParticipant);
    }

    public Task DeleteAsync(ChatParticipant chatParticipant)
    {
        _appDbContext.ChatParticipants.Remove(chatParticipant);
        return Task.CompletedTask;
    }

    public async Task<ChatParticipant?> FirstOrDefaultAsync(
     Expression<Func<ChatParticipant, bool>> predicate)
    {
        return await _appDbContext.ChatParticipants
            .FirstOrDefaultAsync(predicate);
    }

    public Task<ChatParticipant> GetByIdAsync(Guid chatParticipantId)
    {
        throw new NotImplementedException();
    }

    public async Task SaveChangesAsync()
    {
        await _appDbContext.SaveChangesAsync();
    }

    public Task UpdateAsync(ChatParticipant chatParticipant)
    {
        throw new NotImplementedException();
    }
}
