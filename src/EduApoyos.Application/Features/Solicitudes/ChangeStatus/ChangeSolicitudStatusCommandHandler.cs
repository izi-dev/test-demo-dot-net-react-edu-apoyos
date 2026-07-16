using EduApoyos.Application.Common.Abstractions;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Exceptions;
using FluentValidation;

namespace EduApoyos.Application.Features.Solicitudes.ChangeStatus;

/// <summary>
/// Comando para cambiar el estado de una solicitud existente.
/// </summary>
/// <param name="SolicitudId">Identificador de la solicitud a actualizar.</param>
/// <param name="Estado">Estado destino (solo EnRevision, Aprobada o Rechazada según el validator).</param>
/// <param name="Observacion">Observación opcional del asesor (máx. 500 caracteres).</param>
/// <param name="AsesorId">Identificador del asesor que aplica el cambio.</param>
public sealed record ChangeSolicitudStatusCommand(
    Guid SolicitudId,
    EstadoSolicitud Estado,
    string? Observacion,
    Guid AsesorId);

/// <summary>
/// Valida el comando de cambio de estado de solicitud.
/// </summary>
/// <remarks>
/// Reglas de validación (<c>RuleFor</c>):
/// <list type="bullet">
/// <item><c>SolicitudId</c>: no vacío (<c>NotEmpty</c>).</item>
/// <item>
/// <c>Estado</c>: debe ser <c>EnRevision</c>, <c>Aprobada</c> o <c>Rechazada</c>
/// (<c>Must</c>); mensaje: "El estado destino no es válido.".
/// No permite enviar <c>Pendiente</c> como destino vía este comando.
/// </item>
/// <item><c>Observacion</c>: máximo 500 caracteres (<c>MaximumLength(500)</c>); puede ser <c>null</c>.</item>
/// <item><c>AsesorId</c>: no vacío (<c>NotEmpty</c>).</item>
/// </list>
/// Nota: la validez de la transición (Pendiente→EnRevision, EnRevision→Aprobada/Rechazada)
/// la aplica el dominio en <c>SolicitudApoyo.CambiarEstado</c>, no este validator.
/// </remarks>
public sealed class ChangeSolicitudStatusCommandValidator : AbstractValidator<ChangeSolicitudStatusCommand>
{
    /// <summary>
    /// Configura las reglas de FluentValidation para <see cref="ChangeSolicitudStatusCommand"/>.
    /// </summary>
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
/// <remarks>
/// Flujo: valida → carga solicitud con detalle → verifica existencia del asesor →
/// <c>CambiarEstado</c> en dominio → obtiene el nuevo historial →
/// <see cref="ISolicitudRepository.PersistirCambioEstadoAsync"/> →
/// recarga la solicitud (tras Clear del change tracker) → retorna <see cref="SolicitudDto"/>.
/// </remarks>
/// <param name="solicitudes">Puerto de persistencia de solicitudes.</param>
/// <param name="usuarios">Puerto de lectura de usuarios (asesor).</param>
/// <param name="validator">Validador de <see cref="ChangeSolicitudStatusCommand"/>.</param>
public sealed class ChangeSolicitudStatusCommandHandler(
    ISolicitudRepository solicitudes,
    IUsuarioRepository usuarios,
    IValidator<ChangeSolicitudStatusCommand> validator) : ICommandHandler<ChangeSolicitudStatusCommand, SolicitudDto>
{
    /// <summary>
    /// Cambia el estado de la solicitud y retorna el DTO actualizado.
    /// </summary>
    /// <param name="command">Datos del cambio de estado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns><see cref="SolicitudDto"/> de la solicitud tras el cambio.</returns>
    /// <exception cref="FluentValidation.ValidationException">
    /// Si el comando no cumple <see cref="ChangeSolicitudStatusCommandValidator"/>.
    /// </exception>
    /// <exception cref="RecursoNoEncontradoException">
    /// Si no existe la solicitud o el asesor; también si tras persistir no se puede recargar.
    /// </exception>
    /// <exception cref="EduApoyos.Domain.Exceptions.TransicionEstadoInvalidaException">
    /// Si el dominio no permite la transición desde el estado actual al destino.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Si el cambio de estado no generó entrada de historial (inconsistencia de dominio).
    /// </exception>
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
