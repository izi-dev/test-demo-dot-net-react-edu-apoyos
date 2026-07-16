using EduApoyos.Application.Common.Abstractions;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Exceptions;

namespace EduApoyos.Application.Features.Solicitudes.ListByEstudiante;

/// <summary>
/// Consulta las solicitudes del portal del estudiante.
/// </summary>
public sealed record ListSolicitudesByEstudianteQuery(Guid EstudianteId, Guid UsuarioId);

/// <summary>
/// Lista solicitudes validando que el estudiante consulte solo su portal.
/// </summary>
public sealed class ListSolicitudesByEstudianteQueryHandler(
    ISolicitudRepository solicitudes,
    IEstudianteRepository estudiantes) : IQueryHandler<ListSolicitudesByEstudianteQuery, IReadOnlyCollection<SolicitudDto>>
{
    public async Task<IReadOnlyCollection<SolicitudDto>> HandleAsync(
        ListSolicitudesByEstudianteQuery query,
        CancellationToken cancellationToken = default)
    {
        var estudiante = await estudiantes.ObtenerPorUsuarioIdAsync(query.UsuarioId, cancellationToken)
            ?? throw new AccesoRecursoDenegadoException("El usuario no tiene estudiante asociado.");

        if (estudiante.Id != query.EstudianteId)
        {
            throw new AccesoRecursoDenegadoException("Solo puedes consultar tu propio portal.");
        }

        var resultado = await solicitudes.ListarPorEstudianteAsync(estudiante.Id, cancellationToken);
        return resultado.Select(SolicitudMapper.ToDto).ToArray();
    }
}

/// <summary>
/// Consulta las solicitudes del estudiante autenticado sin conocer su identificador de estudiante.
/// </summary>
public sealed record ListMisSolicitudesQuery(Guid UsuarioId);

public sealed class ListMisSolicitudesQueryHandler(
    ISolicitudRepository solicitudes,
    IEstudianteRepository estudiantes) : IQueryHandler<ListMisSolicitudesQuery, IReadOnlyCollection<SolicitudDto>>
{
    public async Task<IReadOnlyCollection<SolicitudDto>> HandleAsync(
        ListMisSolicitudesQuery query,
        CancellationToken cancellationToken = default)
    {
        var estudiante = await estudiantes.ObtenerPorUsuarioIdAsync(query.UsuarioId, cancellationToken)
            ?? throw new AccesoRecursoDenegadoException("El usuario no tiene estudiante asociado.");

        var resultado = await solicitudes.ListarPorEstudianteAsync(estudiante.Id, cancellationToken);
        return resultado.Select(SolicitudMapper.ToDto).ToArray();
    }
}
