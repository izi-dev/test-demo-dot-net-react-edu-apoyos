using EduApoyos.Application.Common.Abstractions;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Exceptions;

namespace EduApoyos.Application.Features.Solicitudes.ListByEstudiante;

/// <summary>
/// Consulta las solicitudes del portal del estudiante.
/// </summary>
/// <param name="EstudianteId">Identificador del estudiante cuyo portal se consulta.</param>
/// <param name="UsuarioId">Usuario autenticado; debe coincidir con el dueño de <paramref name="EstudianteId"/>.</param>
public sealed record ListSolicitudesByEstudianteQuery(Guid EstudianteId, Guid UsuarioId);

/// <summary>
/// Lista solicitudes validando que el estudiante consulte solo su portal.
/// </summary>
/// <remarks>
/// No tiene validator FluentValidation. Compara el perfil resuelto por
/// <see cref="ListSolicitudesByEstudianteQuery.UsuarioId"/> con
/// <see cref="ListSolicitudesByEstudianteQuery.EstudianteId"/> para autorizar.
/// </remarks>
/// <param name="solicitudes">Puerto de lectura de solicitudes.</param>
/// <param name="estudiantes">Puerto de resolución del perfil de estudiante.</param>
public sealed class ListSolicitudesByEstudianteQueryHandler(
    ISolicitudRepository solicitudes,
    IEstudianteRepository estudiantes) : IQueryHandler<ListSolicitudesByEstudianteQuery, IReadOnlyCollection<SolicitudDto>>
{
    /// <summary>
    /// Lista las solicitudes del estudiante indicado tras validar propiedad del portal.
    /// </summary>
    /// <param name="query">Identificadores de estudiante y usuario autenticado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Colección de <see cref="SolicitudDto"/> (posiblemente vacía).</returns>
    /// <exception cref="AccesoRecursoDenegadoException">
    /// Si el usuario no tiene estudiante asociado, o si intenta consultar el portal de otro estudiante.
    /// </exception>
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
/// <param name="UsuarioId">Usuario autenticado cuyo perfil de estudiante se resuelve automáticamente.</param>
public sealed record ListMisSolicitudesQuery(Guid UsuarioId);

/// <summary>
/// Lista las solicitudes del estudiante asociado al usuario autenticado.
/// </summary>
/// <remarks>
/// Variante conveniente de <see cref="ListSolicitudesByEstudianteQuery"/>: no exige
/// <c>EstudianteId</c> en la consulta; lo obtiene desde el perfil del usuario.
/// No tiene validator FluentValidation.
/// </remarks>
/// <param name="solicitudes">Puerto de lectura de solicitudes.</param>
/// <param name="estudiantes">Puerto de resolución del perfil de estudiante.</param>
public sealed class ListMisSolicitudesQueryHandler(
    ISolicitudRepository solicitudes,
    IEstudianteRepository estudiantes) : IQueryHandler<ListMisSolicitudesQuery, IReadOnlyCollection<SolicitudDto>>
{
    /// <summary>
    /// Resuelve el estudiante del usuario y lista sus solicitudes.
    /// </summary>
    /// <param name="query">Identificador del usuario autenticado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Colección de <see cref="SolicitudDto"/> del estudiante (posiblemente vacía).</returns>
    /// <exception cref="AccesoRecursoDenegadoException">
    /// Si el usuario no tiene estudiante asociado.
    /// </exception>
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
