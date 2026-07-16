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
/// Registro de dependencias de infraestructura (persistencia, identidad, seguridad).
/// </summary>
public static class DependencyInjection
{
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
