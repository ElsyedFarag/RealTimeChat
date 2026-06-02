using Microsoft.EntityFrameworkCore;
using RealTimeChat.Domain.Entities;
using RealTimeChat.Infrastructure.Persistence;
using RealTimeChat.Infrastructure.Repositories.Interfaces;

namespace RealTimeChat.Infrastructure.Repositories.Implementation;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<IEnumerable<AppUser>> GetAllAsync(int pageNumber = 1, int pageSize = 20)
    {
        return await context.Users
            .OrderBy(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<AppUser?> GetByIdAsync(string id)
       => await context.Users.FindAsync(id);

    public async Task<AppUser?> GetByUsernameAsync(string username)
       => await context.Users.FirstOrDefaultAsync(u => u.UserName == username);


    public async Task<int> SaveChangesAsync()
      => await context.SaveChangesAsync();

    public async Task<IEnumerable<AppUser>> SearchAsync(string query)
    {
        return await context.Users
        .Where(u =>
            u.UserName!.Contains(query) ||
            u.Email!.Contains(query) ||
            u.PhoneNumber!.Contains(query))
        .ToListAsync();
    }

    public Task UpdateAsync(AppUser user)
      => context.Update(user).State == EntityState.Modified
          ? Task.CompletedTask
          : throw new InvalidOperationException("Failed to update user.");
}
