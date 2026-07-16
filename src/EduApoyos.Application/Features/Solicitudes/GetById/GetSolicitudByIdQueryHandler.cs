using EduApoyos.Application.Common.Abstractions;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Exceptions;

namespace EduApoyos.Application.Features.Solicitudes.GetById;

/// <summary>
/// Consulta el detalle de una solicitud aplicando autorización por recurso.
/// </summary>
public sealed record GetSolicitudByIdQuery(
    Guid SolicitudId,
    Guid UsuarioId,
    RolUsuario Rol);

/// <summary>
/// Obtiene una solicitud validando que el estudiante solo acceda a la suya.
/// </summary>
public sealed class GetSolicitudByIdQueryHandler(
    ISolicitudRepository solicitudes,
    IEstudianteRepository estudiantes) : IQueryHandler<GetSolicitudByIdQuery, SolicitudDto>
{
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
