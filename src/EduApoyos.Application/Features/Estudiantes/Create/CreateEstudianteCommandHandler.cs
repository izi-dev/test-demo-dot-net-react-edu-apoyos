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
/// <param name="UsuarioId">Usuario de identidad al que se asocia el perfil.</param>
/// <param name="NumeroDocumento">Número de documento (máx. 30 caracteres).</param>
/// <param name="TipoDocumento">Tipo de documento de identidad.</param>
/// <param name="ProgramaAcademico">Nombre del programa (máx. 160 caracteres).</param>
/// <param name="Semestre">Semestre académico entre 1 y 15.</param>
public sealed record CreateEstudianteCommand(
    Guid UsuarioId,
    string NumeroDocumento,
    TipoDocumento TipoDocumento,
    string ProgramaAcademico,
    int Semestre);

/// <summary>
/// Valida los datos del comando de creación de estudiante.
/// </summary>
/// <remarks>
/// Reglas de validación (<c>RuleFor</c>):
/// <list type="bullet">
/// <item><c>UsuarioId</c>: no vacío (<c>NotEmpty</c>).</item>
/// <item><c>NumeroDocumento</c>: obligatorio (<c>NotEmpty</c>) y máximo 30 caracteres (<c>MaximumLength(30)</c>).</item>
/// <item><c>ProgramaAcademico</c>: obligatorio (<c>NotEmpty</c>) y máximo 160 caracteres (<c>MaximumLength(160)</c>).</item>
/// <item><c>Semestre</c>: inclusive entre 1 y 15 (<c>InclusiveBetween(1, 15)</c>).</item>
/// <item><c>TipoDocumento</c>: sin regla FluentValidation explícita; se acepta el valor enum recibido.</item>
/// </list>
/// </remarks>
public sealed class CreateEstudianteCommandValidator : AbstractValidator<CreateEstudianteCommand>
{
    /// <summary>
    /// Configura las reglas de FluentValidation para <see cref="CreateEstudianteCommand"/>.
    /// </summary>
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
/// <remarks>
/// Flujo: valida el comando → verifica que el usuario exista →
/// crea el agregado vía <see cref="Estudiante.Crear"/> → persiste con
/// <see cref="IEstudianteRepository.AgregarAsync"/> y <see cref="IUnitOfWork.SaveChangesAsync"/> →
/// retorna <see cref="EstudianteDto"/>.
/// </remarks>
/// <param name="estudiantes">Puerto de persistencia de estudiantes.</param>
/// <param name="usuarios">Puerto de lectura de usuarios de dominio.</param>
/// <param name="unitOfWork">Unidad de trabajo para confirmar la inserción.</param>
/// <param name="validator">Validador de <see cref="CreateEstudianteCommand"/>.</param>
public sealed class CreateEstudianteCommandHandler(
    IEstudianteRepository estudiantes,
    IUsuarioRepository usuarios,
    IUnitOfWork unitOfWork,
    IValidator<CreateEstudianteCommand> validator) : ICommandHandler<CreateEstudianteCommand, EstudianteDto>
{
    /// <summary>
    /// Crea el perfil de estudiante y lo proyecta a DTO.
    /// </summary>
    /// <param name="command">Datos del perfil académico.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns><see cref="EstudianteDto"/> del estudiante creado.</returns>
    /// <exception cref="FluentValidation.ValidationException">
    /// Si el comando no cumple <see cref="CreateEstudianteCommandValidator"/>.
    /// </exception>
    /// <exception cref="RecursoNoEncontradoException">
    /// Si no existe el usuario indicado en <see cref="CreateEstudianteCommand.UsuarioId"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Si el dominio rechaza datos inválidos al construir el agregado (documento, programa, etc.).
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Si el semestre está fuera del rango permitido por el dominio.
    /// </exception>
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

    /// <summary>
    /// Proyecta la entidad de dominio a <see cref="EstudianteDto"/>.
    /// </summary>
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
