using EduApoyos.Application.Common.Abstractions;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Exceptions;

namespace EduApoyos.Application.Features.Solicitudes.GetById;

/// <summary>
/// Consulta el detalle de una solicitud aplicando autorización por recurso.
/// </summary>
/// <param name="SolicitudId">Identificador de la solicitud a consultar.</param>
/// <param name="UsuarioId">Usuario autenticado que realiza la consulta.</param>
/// <param name="Rol">Rol del usuario (Estudiante o Asesor).</param>
public sealed record GetSolicitudByIdQuery(
    Guid SolicitudId,
    Guid UsuarioId,
    RolUsuario Rol);

/// <summary>
/// Obtiene una solicitud validando que el estudiante solo acceda a la suya.
/// </summary>
/// <remarks>
/// No tiene validator FluentValidation. Los asesores pueden consultar cualquier solicitud;
/// los estudiantes solo las propias (vía <c>PerteneceA</c>).
/// </remarks>
/// <param name="solicitudes">Puerto de lectura de solicitudes.</param>
/// <param name="estudiantes">Puerto de resolución del perfil de estudiante.</param>
public sealed class GetSolicitudByIdQueryHandler(
    ISolicitudRepository solicitudes,
    IEstudianteRepository estudiantes) : IQueryHandler<GetSolicitudByIdQuery, SolicitudDto>
{
    /// <summary>
    /// Obtiene el detalle de una solicitud autorizando por rol y propiedad.
    /// </summary>
    /// <param name="query">Identificadores de solicitud, usuario y rol.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns><see cref="SolicitudDto"/> con historial y datos del estudiante.</returns>
    /// <exception cref="RecursoNoEncontradoException">
    /// Si la solicitud no existe.
    /// </exception>
    /// <exception cref="AccesoRecursoDenegadoException">
    /// Si el rol es Estudiante y el usuario no tiene perfil, o si la solicitud no le pertenece.
    /// </exception>
    public async Task<SolicitudDto> HandleAsync(GetSolicitudByIdQuery query, CancellationToken cancellationToken = default)
    {
        var solicitud = await solicitudes.ObtenerPorIdConDetalleAsync(query.SolicitudId, cancellationToken)
            ?? throw new RecursoNoEncontradoException("solicitud", query.SolicitudId);

        if (query.Rol == RolUsuario.Estudiante)
        {
            var estudiante = await estudiantes.ObtenerPorUsuarioIdAsync(query.UsuarioId, cancellationToken)
                ?? throw new AccesoRecursoDenegadoException("El usuario no tiene estudiante asociado.");

            if (!solicitud.PerteneceA(estudiante.Id))
            {
                throw new AccesoRecursoDenegadoException("Solo puedes consultar tus propias solicitudes.");
            }
        }

        return SolicitudMapper.ToDto(solicitud);
    }
}
