using EduApoyos.Application.Common.Abstractions;
using EduApoyos.Application.Features.Auth;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Enums;
using FluentValidation;

namespace EduApoyos.Application.Features.Auth.Register;

/// <summary>
/// Comando para registrar un nuevo usuario en el sistema.
/// </summary>
/// <param name="NombreCompleto">Nombre visible del usuario.</param>
/// <param name="Email">Correo único de acceso.</param>
/// <param name="Password">Contraseña en texto plano (mínimo 8 caracteres según el validator).</param>
/// <param name="Rol">Rol de dominio a asignar.</param>
public sealed record RegisterCommand(
    string NombreCompleto,
    string Email,
    string Password,
    RolUsuario Rol);

/// <summary>
/// Valida los datos mínimos del comando de registro.
/// </summary>
/// <remarks>
/// Reglas de validación (<c>RuleFor</c>):
/// <list type="bullet">
/// <item><c>NombreCompleto</c>: obligatorio (<c>NotEmpty</c>) y máximo 160 caracteres (<c>MaximumLength(160)</c>).</item>
/// <item><c>Email</c>: obligatorio (<c>NotEmpty</c>), formato válido (<c>EmailAddress</c>) y máximo 256 caracteres (<c>MaximumLength(256)</c>).</item>
/// <item><c>Password</c>: obligatorio (<c>NotEmpty</c>) y longitud mínima 8 (<c>MinimumLength(8)</c>).</item>
/// <item><c>Rol</c>: sin regla FluentValidation explícita; se acepta el valor enum recibido.</item>
/// </list>
/// </remarks>
public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    /// <summary>
    /// Configura las reglas de FluentValidation para <see cref="RegisterCommand"/>.
    /// </summary>
    public RegisterCommandValidator()
    {
        RuleFor(x => x.NombreCompleto).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

/// <summary>
/// Maneja el caso de uso de registro de usuario y emisión de JWT.
/// </summary>
/// <remarks>
/// Flujo: valida el comando → registra vía <see cref="IIdentityService"/> →
/// si hay errores de Identity los traduce a <c>ValidationException</c> →
/// genera JWT → retorna <see cref="AuthResultDto"/>.
/// </remarks>
/// <param name="identityService">Puerto de registro e identidad.</param>
/// <param name="jwtTokenGenerator">Puerto de emisión de tokens.</param>
/// <param name="validator">Validador de <see cref="RegisterCommand"/>.</param>
public sealed class RegisterCommandHandler(
    IIdentityService identityService,
    IJwtTokenGenerator jwtTokenGenerator,
    IValidator<RegisterCommand> validator) : ICommandHandler<RegisterCommand, AuthResultDto>
{
    /// <summary>
    /// Registra un usuario nuevo y emite un token JWT.
    /// </summary>
    /// <param name="command">Datos de registro.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>
    /// <see cref="AuthResultDto"/> con token, expiración y datos del usuario creado.
    /// </returns>
    /// <exception cref="FluentValidation.ValidationException">
    /// Si el comando no cumple <see cref="RegisterCommandValidator"/>, o si Identity
    /// rechaza el registro (email duplicado, política de contraseña, etc.).
    /// </exception>
    public async Task<AuthResultDto> HandleAsync(RegisterCommand command, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var resultado = await identityService.RegistrarAsync(
            new RegistroUsuarioSolicitud(
                NombreCompleto: command.NombreCompleto,
                Email: command.Email,
                Password: command.Password,
                Rol: command.Rol),
            cancellationToken);

        if (!resultado.Exitoso || resultado.Usuario is null)
        {
            throw new ValidationException(
                resultado.Errores?.SelectMany(x => x.Value.Select(msg =>
                    new FluentValidation.Results.ValidationFailure(x.Key, msg))) ?? []);
        }

        var token = jwtTokenGenerator.Generar(resultado.Usuario);

        return new AuthResultDto(
            Token: token.Valor,
            ExpiresAt: token.ExpiraEn,
            UserId: resultado.Usuario.Id,
            NombreCompleto: resultado.Usuario.NombreCompleto,
            Email: resultado.Usuario.Email,
            Rol: resultado.Usuario.Rol);
    }
}
