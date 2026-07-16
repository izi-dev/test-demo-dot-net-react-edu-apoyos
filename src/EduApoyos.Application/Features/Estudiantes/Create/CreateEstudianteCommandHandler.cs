using EduApoyos.Application.Common.Abstractions;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Exceptions;
using FluentValidation;

namespace EduApoyos.Application.Features.Estudiantes.Create;

/// <summary>
/// Comando para registrar el perfil académico de un estudiante.
/// </summary>
public sealed record CreateEstudianteCommand(
    Guid UsuarioId,
    string NumeroDocumento,
    TipoDocumento TipoDocumento,
    string ProgramaAcademico,
    int Semestre);

public sealed class CreateEstudianteCommandValidator : AbstractValidator<CreateEstudianteCommand>
{
    public CreateEstudianteCommandValidator()
    {
        RuleFor(x => x.UsuarioId).NotEmpty();
        RuleFor(x => x.NumeroDocumento).NotEmpty().MaximumLength(30);
        RuleFor(x => x.ProgramaAcademico).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Semestre).InclusiveBetween(1, 15);
    }
}

/// <summary>
/// Crea un estudiante asociado a un usuario existente.
/// </summary>
public sealed class CreateEstudianteCommandHandler(
    IEstudianteRepository estudiantes,
    IUsuarioRepository usuarios,
    IUnitOfWork unitOfWork,
    IValidator<CreateEstudianteCommand> validator) : ICommandHandler<CreateEstudianteCommand, EstudianteDto>
{
    public async Task<EstudianteDto> HandleAsync(CreateEstudianteCommand command, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var usuario = await usuarios.ObtenerPorIdAsync(command.UsuarioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException("usuario", command.UsuarioId);

        var estudiante = Estudiante.Crear(
            usuarioId: command.UsuarioId,
            usuario: usuario,
            numeroDocumento: command.NumeroDocumento,
            tipoDocumento: command.TipoDocumento,
            programaAcademico: command.ProgramaAcademico,
            semestre: command.Semestre);

        await estudiantes.AgregarAsync(estudiante, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        estudiante.Usuario = usuario;
        return Map(estudiante);
    }

    private static EstudianteDto Map(Estudiante estudiante) =>
        new(
            Id: estudiante.Id,
            UsuarioId: estudiante.UsuarioId,
            NombreCompleto: estudiante.Usuario?.NombreCompleto ?? string.Empty,
            Email: estudiante.Usuario?.Email ?? string.Empty,
            NumeroDocumento: estudiante.NumeroDocumento,
            TipoDocumento: estudiante.TipoDocumento,
            ProgramaAcademico: estudiante.ProgramaAcademico,
            Semestre: estudiante.Semestre);
}
