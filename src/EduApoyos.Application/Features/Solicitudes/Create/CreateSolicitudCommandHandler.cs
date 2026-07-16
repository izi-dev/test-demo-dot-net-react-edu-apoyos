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
public sealed record CreateSolicitudCommand(
    Guid? EstudianteId,
    TipoApoyo TipoApoyo,
    decimal MontoSolicitado,
    string Descripcion,
    Guid UsuarioSolicitanteId,
    RolUsuario RolSolicitante);

public sealed class CreateSolicitudCommandValidator : AbstractValidator<CreateSolicitudCommand>
{
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
public sealed class CreateSolicitudCommandHandler(
    ISolicitudRepository solicitudes,
    IEstudianteRepository estudiantes,
    IUnitOfWork unitOfWork,
    IValidator<CreateSolicitudCommand> validator) : ICommandHandler<CreateSolicitudCommand, SolicitudDto>
{
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
