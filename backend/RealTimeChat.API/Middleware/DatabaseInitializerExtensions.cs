using RealTimeChat.Infrastructure.Persistence.Seed;

namespace RealTimeChat.API.Middleware;

public static class DatabaseInitializerExtensions
{
    public static async Task<IApplicationBuilder> InitializeDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
        await initializer.InitializeAsync();
        return app;
    }
}
