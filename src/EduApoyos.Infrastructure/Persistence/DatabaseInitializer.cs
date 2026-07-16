using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EduApoyos.Infrastructure.Persistence;

/// <summary>
/// Extensiones de arranque que aplican migraciones EF Core y ejecutan el seed de datos.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Aplica migraciones pendientes y, a continuación, inserta datos iniciales de demostración.
    /// </summary>
    /// <param name="host">Host de la aplicación desde el que se resuelve el alcance de servicios.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Una tarea que completa cuando migraciones y seed han finalizado.</returns>
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
