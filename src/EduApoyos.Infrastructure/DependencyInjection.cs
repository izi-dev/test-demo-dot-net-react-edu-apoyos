using EduApoyos.Application.Ports;
using EduApoyos.Infrastructure.Identity;
using EduApoyos.Infrastructure.Persistence;
using EduApoyos.Infrastructure.Repositories;
using EduApoyos.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduApoyos.Infrastructure;

/// <summary>
/// Extensiones de registro de dependencias de la capa de infraestructura
/// (persistencia, identidad, repositorios y seguridad JWT).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra el contexto EF Core, Identity (solo núcleo, sin cookies),
    /// repositorios, unit of work, servicios de identidad/JWT y el seeder.
    /// </summary>
    /// <param name="services">Colección de servicios de la aplicación.</param>
    /// <param name="configuration">Configuración desde la que se leen conexión y JWT.</param>
    /// <returns>La misma colección de servicios para encadenar llamadas.</returns>
    /// <exception cref="InvalidOperationException">
    /// Se lanza cuando no hay cadena de conexión (<c>ConnectionStrings:Default</c>
    /// ni <c>CONNECTION_STRING</c>).
    /// </exception>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = ResolveConnectionString(configuration)
            ?? throw new InvalidOperationException("Falta CONNECTION_STRING.");

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddDbContext<EduApoyosDbContext>(options =>
            options.UseNpgsql(connectionString));


        // AddIdentityCore (sin cookies): la API solo autentica con JWT.
        // AddIdentity registraría cookies y redirigiría a /Account/Login.
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<EduApoyosDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IEstudianteRepository, EstudianteRepository>();
        services.AddScoped<ISolicitudRepository, SolicitudRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EduApoyosDbContext>());
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<DataSeeder>();

        return services;
    }

    /// <summary>
    /// Obtiene y normaliza la cadena de conexión desde configuración.
    /// </summary>
    /// <param name="configuration">Configuración de la aplicación.</param>
    /// <returns>
    /// Cadena limpia (sin comillas envolventes) o <c>null</c> si no está definida.
    /// </returns>
    private static string? ResolveConnectionString(IConfiguration configuration)
    {
        var raw = configuration.GetConnectionString("Default")
            ?? configuration["CONNECTION_STRING"];

        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        // Algunos paneles dejan comillas literales alrededor del valor.
        return raw.Trim().Trim('"').Trim('\'');
    }
}
