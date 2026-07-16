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
public static class DependencyInjection
{
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
