using EduApoyos.Application.Common.Abstractions;
using EduApoyos.Application.Features.Auth;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Enums;
using FluentValidation;

namespace EduApoyos.Application.Features.Auth.Register;

/// <summary>
/// Comando para registrar un nuevo usuario en el sistema.
/// </summary>
public sealed record RegisterCommand(
    string NombreCompleto,
    string Email,
    string Password,
    RolUsuario Rol);

/// <summary>
/// Valida los datos mínimos del comando de registro.
/// </summary>
public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
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
public sealed class RegisterCommandHandler(
    IIdentityService identityService,
    IJwtTokenGenerator jwtTokenGenerator,
    IValidator<RegisterCommand> validator) : ICommandHandler<RegisterCommand, AuthResultDto>
{
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
