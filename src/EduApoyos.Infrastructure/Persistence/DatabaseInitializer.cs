using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EduApoyos.Infrastructure.Persistence;

/// <summary>
/// Aplica migraciones y seed al arrancar la aplicación.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task MigrateAndSeedAsync(
        this IHost host,
        CancellationToken cancellationToken = default)
    {
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EduApoyosDbContext>();

        await context.Database.MigrateAsync(cancellationToken: cancellationToken);

        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync(cancellationToken: cancellationToken);
    }
}
