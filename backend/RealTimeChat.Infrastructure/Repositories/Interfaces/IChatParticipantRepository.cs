using RealTimeChat.Domain.Entities;
using System.Linq.Expressions;

namespace RealTimeChat.Infrastructure.Repositories.Interfaces;

public interface IChatParticipantRepository
{
    Task AddAsync(ChatParticipant chatParticipant);
    Task UpdateAsync(ChatParticipant chatParticipant);
    Task DeleteAsync(ChatParticipant chatParticipant);
    Task<ChatParticipant> GetByIdAsync(Guid chatParticipantId);
    Task<ChatParticipant?> FirstOrDefaultAsync(Expression<Func<ChatParticipant, bool>> predicate);
    Task SaveChangesAsync();
}
