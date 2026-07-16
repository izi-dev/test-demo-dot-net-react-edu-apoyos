using EduApoyos.Application.Common.Abstractions;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Exceptions;
using EduApoyos.Domain.ValueObjects;
using FluentValidation;

namespace EduApoyos.Application.Features.Solicitudes.Create;

/// <summary>
/// Comando para crear una solicitud de apoyo económico.
/// </summary>
/// <param name="EstudianteId">
/// Identificador del estudiante destino. Obligatorio cuando el solicitante es asesor;
/// se ignora (se resuelve por usuario) cuando el solicitante es estudiante.
/// </param>
/// <param name="TipoApoyo">Tipo de apoyo solicitado.</param>
/// <param name="MontoSolicitado">Monto mayor que 0 y hasta 100.000.000.</param>
/// <param name="Descripcion">Descripción obligatoria (máx. 1000 caracteres).</param>
/// <param name="UsuarioSolicitanteId">Usuario autenticado que crea la solicitud.</param>
/// <param name="RolSolicitante">Rol del usuario autenticado (Estudiante o Asesor).</param>
public sealed record CreateSolicitudCommand(
    Guid? EstudianteId,
    TipoApoyo TipoApoyo,
    decimal MontoSolicitado,
    string Descripcion,
    Guid UsuarioSolicitanteId,
    RolUsuario RolSolicitante);

/// <summary>
/// Valida los datos del comando de creación de solicitud.
/// </summary>
/// <remarks>
/// Reglas de validación (<c>RuleFor</c>):
/// <list type="bullet">
/// <item>
/// <c>EstudianteId</c>: no vacío (<c>NotEmpty</c>) solo cuando
/// <c>RolSolicitante == RolUsuario.Asesor</c> (<c>When</c>); mensaje:
/// "El estudiante es obligatorio para el asesor.".
/// </item>
/// <item>
/// <c>MontoSolicitado</c>: mayor que 0 (<c>GreaterThan(0)</c>) y menor o igual a
/// 100.000.000 (<c>LessThanOrEqualTo(100_000_000)</c>).
/// </item>
/// <item>
/// <c>Descripcion</c>: obligatorio (<c>NotEmpty</c>) y máximo 1000 caracteres (<c>MaximumLength(1000)</c>).
/// </item>
/// <item>
/// <c>TipoApoyo</c>, <c>UsuarioSolicitanteId</c> y <c>RolSolicitante</c>: sin reglas FluentValidation explícitas.
/// </item>
/// </list>
/// </remarks>
public sealed class CreateSolicitudCommandValidator : AbstractValidator<CreateSolicitudCommand>
{
    /// <summary>
    /// Configura las reglas de FluentValidation para <see cref="CreateSolicitudCommand"/>.
    /// </summary>
    public CreateSolicitudCommandValidator()
    {
        RuleFor(x => x.EstudianteId)
            .NotEmpty()
            .When(x => x.RolSolicitante == RolUsuario.Asesor)
            .WithMessage("El estudiante es obligatorio para el asesor.");
        RuleFor(x => x.MontoSolicitado).GreaterThan(0).LessThanOrEqualTo(100_000_000);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(1000);
    }
}

/// <summary>
/// Crea una solicitud en estado pendiente y registra el historial inicial.
/// </summary>
/// <remarks>
/// Flujo: valida → resuelve estudiante (por usuario si es Estudiante, o por Id si es Asesor) →
/// crea agregado con <see cref="SolicitudApoyo.Crear"/> →
/// <see cref="ISolicitudRepository.AgregarAsync"/> + <see cref="IUnitOfWork.SaveChangesAsync"/> →
/// retorna <see cref="SolicitudDto"/>.
/// </remarks>
/// <param name="solicitudes">Puerto de persistencia de solicitudes.</param>
/// <param name="estudiantes">Puerto de lectura de estudiantes.</param>
/// <param name="unitOfWork">Unidad de trabajo para confirmar la inserción.</param>
/// <param name="validator">Validador de <see cref="CreateSolicitudCommand"/>.</param>
public sealed class CreateSolicitudCommandHandler(
    ISolicitudRepository solicitudes,
    IEstudianteRepository estudiantes,
    IUnitOfWork unitOfWork,
    IValidator<CreateSolicitudCommand> validator) : ICommandHandler<CreateSolicitudCommand, SolicitudDto>
{
    /// <summary>
    /// Crea la solicitud de apoyo y la proyecta a DTO.
    /// </summary>
    /// <param name="command">Datos de la nueva solicitud.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns><see cref="SolicitudDto"/> de la solicitud creada (estado pendiente).</returns>
    /// <exception cref="FluentValidation.ValidationException">
    /// Si el comando no cumple <see cref="CreateSolicitudCommandValidator"/>, o si tras
    /// la resolución de rol no hay <c>EstudianteId</c> ("Debe indicar el estudiante de la solicitud.").
    /// </exception>
    /// <exception cref="AccesoRecursoDenegadoException">
    /// Si el rol es Estudiante y el usuario no tiene perfil de estudiante asociado.
    /// </exception>
    /// <exception cref="RecursoNoEncontradoException">
    /// Si el estudiante indicado no existe.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Si el value object <c>MontoSolicitado</c> del dominio rechaza el monto.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Si el value object <c>DescripcionSolicitud</c> del dominio rechaza la descripción.
    /// </exception>
    public async Task<SolicitudDto> HandleAsync(CreateSolicitudCommand command, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var estudianteId = command.EstudianteId;

        if (command.RolSolicitante == RolUsuario.Estudiante)
        {
            var perfil = await estudiantes.ObtenerPorUsuarioIdAsync(command.UsuarioSolicitanteId, cancellationToken)
                ?? throw new AccesoRecursoDenegadoException("El usuario no tiene estudiante asociado.");

            estudianteId = perfil.Id;
        }

        if (!estudianteId.HasValue)
        {
            throw new ValidationException("Debe indicar el estudiante de la solicitud.");
        }

        var estudiante = await estudiantes.ObtenerPorIdAsync(estudianteId.Value, cancellationToken)
            ?? throw new RecursoNoEncontradoException("estudiante", estudianteId.Value);

        var solicitud = SolicitudApoyo.Crear(
            estudianteId: estudiante.Id,
            estudiante: estudiante,
            tipoApoyo: command.TipoApoyo,
            monto: MontoSolicitado.Crear(command.MontoSolicitado),
            descripcion: DescripcionSolicitud.Crear(command.Descripcion),
            usuarioCreadorId: command.UsuarioSolicitanteId);

        await solicitudes.AgregarAsync(solicitud, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return SolicitudMapper.ToDto(solicitud);
    }
}
