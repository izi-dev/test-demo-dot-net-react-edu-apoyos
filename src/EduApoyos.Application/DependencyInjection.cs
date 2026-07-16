using EduApoyos.Application.Features.Auth.Login;
using EduApoyos.Application.Features.Auth.Register;
using EduApoyos.Application.Features.Constancias;
using EduApoyos.Application.Features.Estudiantes.Create;
using EduApoyos.Application.Features.Estudiantes.List;
using EduApoyos.Application.Features.Solicitudes.ChangeStatus;
using EduApoyos.Application.Features.Solicitudes.Create;
using EduApoyos.Application.Features.Solicitudes.GetById;
using EduApoyos.Application.Features.Solicitudes.List;
using EduApoyos.Application.Features.Solicitudes.ListByEstudiante;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EduApoyos.Application;

/// <summary>
/// Registro de dependencias de la capa de aplicación por features (vertical slices).
/// </summary>
/// <remarks>
/// Expone <see cref="AddApplication"/> para que la API (u host) registre validators
/// y handlers de comandos/consultas sin conocer detalles de cada feature.
/// </remarks>
public static class DependencyInjection
{
    /// <summary>
    /// Registra validators de FluentValidation y handlers de la capa de aplicación.
    /// </summary>
    /// <param name="services">Colección de servicios del contenedor DI.</param>
    /// <returns>La misma colección para encadenar registros.</returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Registra todos los <c>AbstractValidator&lt;T&gt;</c> del ensamblado.</item>
    /// <item>
    /// Registra como <c>Scoped</c> los handlers: Register, Login, CreateEstudiante,
    /// ListEstudiantes, CreateSolicitud, ChangeSolicitudStatus, GetSolicitudById,
    /// ListSolicitudes, ListSolicitudesByEstudiante, ListMisSolicitudes y GenerateConstancia.
    /// </item>
    /// <item>No registra puertos de infraestructura; eso corresponde a la capa Infrastructure.</item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<RegisterCommandHandler>();
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<CreateEstudianteCommandHandler>();
        services.AddScoped<ListEstudiantesQueryHandler>();
        services.AddScoped<CreateSolicitudCommandHandler>();
        services.AddScoped<ChangeSolicitudStatusCommandHandler>();
        services.AddScoped<GetSolicitudByIdQueryHandler>();
        services.AddScoped<ListSolicitudesQueryHandler>();
        services.AddScoped<ListSolicitudesByEstudianteQueryHandler>();
        services.AddScoped<ListMisSolicitudesQueryHandler>();
        services.AddScoped<GenerateConstanciaQueryHandler>();

        return services;
    }
}
