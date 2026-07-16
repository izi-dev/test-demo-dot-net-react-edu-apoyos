using EduApoyos.Application.Common.Abstractions;
using EduApoyos.Application.Features.Auth;
using EduApoyos.Application.Ports;
using FluentValidation;

namespace EduApoyos.Application.Features.Auth.Login;

/// <summary>
/// Comando para autenticar un usuario existente.
/// </summary>
/// <param name="Email">Correo electrónico de acceso.</param>
/// <param name="Password">Contraseña en texto plano.</param>
public sealed record LoginCommand(string Email, string Password);

/// <summary>
/// Valida credenciales mínimas del comando de login.
/// </summary>
/// <remarks>
/// Reglas de validación (<c>RuleFor</c>):
/// <list type="bullet">
/// <item><c>Email</c>: obligatorio (<c>NotEmpty</c>) y formato de correo válido (<c>EmailAddress</c>).</item>
/// <item><c>Password</c>: obligatorio (<c>NotEmpty</c>).</item>
/// </list>
/// </remarks>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>
    /// Configura las reglas de FluentValidation para <see cref="LoginCommand"/>.
    /// </summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

/// <summary>
/// Maneja el caso de uso de inicio de sesión y emisión de JWT.
/// </summary>
/// <remarks>
/// Flujo: valida el comando → autentica vía <see cref="IIdentityService"/> →
/// genera JWT vía <see cref="IJwtTokenGenerator"/> → retorna <see cref="AuthResultDto"/>.
/// </remarks>
/// <param name="identityService">Puerto de autenticación.</param>
/// <param name="jwtTokenGenerator">Puerto de emisión de tokens.</param>
/// <param name="validator">Validador de <see cref="LoginCommand"/>.</param>
public sealed class LoginCommandHandler(
    IIdentityService identityService,
    IJwtTokenGenerator jwtTokenGenerator,
    IValidator<LoginCommand> validator) : ICommandHandler<LoginCommand, AuthResultDto>
{
    /// <summary>
    /// Autentica al usuario y emite un token JWT.
    /// </summary>
    /// <param name="command">Credenciales de acceso.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>
    /// <see cref="AuthResultDto"/> con token, expiración y datos del usuario.
    /// </returns>
    /// <exception cref="FluentValidation.ValidationException">
    /// Si el comando no cumple las reglas de <see cref="LoginCommandValidator"/>.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// Si el correo o la contraseña no son correctos (el puerto devuelve <c>null</c>).
    /// </exception>
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
