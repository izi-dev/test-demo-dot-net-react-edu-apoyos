using EduApoyos.Application.Common.Abstractions;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Exceptions;
using FluentValidation;

namespace EduApoyos.Application.Features.Solicitudes.ChangeStatus;

/// <summary>
/// Comando para cambiar el estado de una solicitud existente.
/// </summary>
public sealed record ChangeSolicitudStatusCommand(
    Guid SolicitudId,
    EstadoSolicitud Estado,
    string? Observacion,
    Guid AsesorId);

public sealed class ChangeSolicitudStatusCommandValidator : AbstractValidator<ChangeSolicitudStatusCommand>
{
    public ChangeSolicitudStatusCommandValidator()
    {
        RuleFor(x => x.SolicitudId).NotEmpty();
        RuleFor(x => x.Estado)
            .Must(x => x is EstadoSolicitud.EnRevision or EstadoSolicitud.Aprobada or EstadoSolicitud.Rechazada)
            .WithMessage("El estado destino no es válido.");
        RuleFor(x => x.Observacion).MaximumLength(500);
        RuleFor(x => x.AsesorId).NotEmpty();
    }
}

/// <summary>
/// Aplica una transición de estado validada por el dominio.
/// </summary>
public sealed class ChangeSolicitudStatusCommandHandler(
    ISolicitudRepository solicitudes,
    IUsuarioRepository usuarios,
    IValidator<ChangeSolicitudStatusCommand> validator) : ICommandHandler<ChangeSolicitudStatusCommand, SolicitudDto>
{
    public async Task<SolicitudDto> HandleAsync(ChangeSolicitudStatusCommand command, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var solicitud = await solicitudes.ObtenerPorIdConDetalleAsync(command.SolicitudId, cancellationToken)
            ?? throw new RecursoNoEncontradoException("solicitud", command.SolicitudId);

        _ = await usuarios.ObtenerPorIdAsync(command.AsesorId, cancellationToken)
            ?? throw new RecursoNoEncontradoException("usuario", command.AsesorId);

        solicitud.CambiarEstado(
            nuevoEstado: command.Estado,
            usuarioId: command.AsesorId,
            observacion: command.Observacion,
            asesorId: command.AsesorId);

        var nuevoHistorial = solicitud.UltimoHistorial()
            ?? throw new InvalidOperationException("El cambio de estado no generó historial.");

        await solicitudes.PersistirCambioEstadoAsync(solicitud, nuevoHistorial, cancellationToken);

        // Tras Clear() del change tracker, recargar para devolver el DTO completo.
        var actualizada = await solicitudes.ObtenerPorIdConDetalleAsync(command.SolicitudId, cancellationToken)
            ?? throw new RecursoNoEncontradoException("solicitud", command.SolicitudId);

        return SolicitudMapper.ToDto(actualizada);
    }
}
