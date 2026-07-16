using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduApoyos.Infrastructure.Persistence;

/// <summary>
/// Utilidades para inicializar la base de datos al arrancar la aplicación.
/// </summary>
public static class DatabaseInitializer
{
    private const int MaxAttempts = 10;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Aplica migraciones pendientes y ejecuta el sembrado de datos iniciales.
    /// Reintenta ante fallos transitorios de red/DNS al conectar con PostgreSQL.
    /// </summary>
    /// <param name="host">Host de la aplicación con el contenedor de servicios.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    public static async Task MigrateAndSeedAsync(
        this IHost host,
        CancellationToken cancellationToken = default)
    {
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EduApoyosDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("EduApoyos.DatabaseInitializer");

        var hostName = TryGetHostFromConnectionString(context.Database.GetConnectionString());
        if (!string.IsNullOrWhiteSpace(hostName))
        {
            logger.LogInformation("Conectando a PostgreSQL en Host={Host}", hostName);
        }

        Exception? lastError = null;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                logger.LogInformation(
                    "Aplicando migraciones (intento {Attempt}/{MaxAttempts})...",
                    attempt,
                    MaxAttempts);

                await context.Database.MigrateAsync(cancellationToken: cancellationToken);

                var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                await seeder.SeedAsync(cancellationToken: cancellationToken);

                logger.LogInformation("Base de datos migrada y sembrada correctamente.");
                return;
            }
            catch (Exception ex) when (IsTransientConnectionError(ex) && attempt < MaxAttempts)
            {
                lastError = ex;
                logger.LogWarning(
                    ex,
                    "No se pudo conectar a PostgreSQL (intento {Attempt}/{MaxAttempts}). " +
                    "Verifica ConnectionStrings__Default / CONNECTION_STRING: host resoluble, red compartida y que el servicio esté arriba. Reintento en {DelaySeconds}s...",
                    attempt,
                    MaxAttempts,
                    RetryDelay.TotalSeconds);

                await Task.Delay(RetryDelay, cancellationToken);
            }
            catch (Exception ex)
            {
                lastError = ex;
                break;
            }
        }

        throw new InvalidOperationException(
            "No se pudo conectar a PostgreSQL para aplicar migraciones. " +
            "Revisa que el Host de la cadena de conexión sea resoluble desde el contenedor del API " +
            "(mismo Docker network o hostname público correcto) y que el servicio de base de datos esté en ejecución.",
            lastError);
    }

    private static bool IsTransientConnectionError(Exception ex)
    {
        for (var current = ex; current is not null; current = current.InnerException!)
        {
            if (current is System.Net.Sockets.SocketException
                or TimeoutException
                or Npgsql.NpgsqlException)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Extrae solo el Host de la cadena (sin contraseña) para logs de diagnóstico.
    /// </summary>
    private static string? TryGetHostFromConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        foreach (var part in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = part.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            var key = part[..separator].Trim();
            if (key.Equals("Host", StringComparison.OrdinalIgnoreCase)
                || key.Equals("Server", StringComparison.OrdinalIgnoreCase))
            {
                return part[(separator + 1)..].Trim();
            }
        }

        return null;
    }
}
