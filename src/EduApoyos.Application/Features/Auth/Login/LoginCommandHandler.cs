using EduApoyos.Application.Common.Abstractions;
using EduApoyos.Application.Features.Auth;
using EduApoyos.Application.Ports;
using FluentValidation;

namespace EduApoyos.Application.Features.Auth.Login;

/// <summary>
/// Comando para autenticar un usuario existente.
/// </summary>
public sealed record LoginCommand(string Email, string Password);

/// <summary>
/// Valida credenciales mínimas del comando de login.
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

/// <summary>
/// Maneja el caso de uso de inicio de sesión y emisión de JWT.
/// </summary>
public sealed class LoginCommandHandler(
    IIdentityService identityService,
    IJwtTokenGenerator jwtTokenGenerator,
    IValidator<LoginCommand> validator) : ICommandHandler<LoginCommand, AuthResultDto>
{
    public async Task<AuthResultDto> HandleAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var usuario = await identityService.AutenticarAsync(command.Email, command.Password, cancellationToken)
            ?? throw new UnauthorizedAccessException("El correo o la contraseña no son correctos.");

        var token = jwtTokenGenerator.Generar(usuario);

        return new AuthResultDto(
            Token: token.Valor,
            ExpiresAt: token.ExpiraEn,
            UserId: usuario.Id,
            NombreCompleto: usuario.NombreCompleto,
            Email: usuario.Email,
            Rol: usuario.Rol);
    }
}
