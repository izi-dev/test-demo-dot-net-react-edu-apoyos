using EduApoyos.Api.Auth;
using EduApoyos.Application.Features.Auth;
using EduApoyos.Application.Features.Auth.Login;
using EduApoyos.Application.Features.Auth.Register;
using Microsoft.AspNetCore.Mvc;

namespace EduApoyos.Api.Controllers;

/// <summary>
/// Endpoints públicos de autenticación: registro e inicio de sesión.
/// Solo traduce HTTP a casos de uso de la capa de aplicación.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    RegisterCommandHandler registerHandler,
    LoginCommandHandler loginHandler) : ControllerBase
{
    /// <summary>
    /// Registra un nuevo usuario y devuelve el resultado de autenticación (token incluido).
    /// </summary>
    /// <param name="request">Datos de registro (nombre, correo, contraseña y rol).</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resultado de autenticación con token JWT.</returns>
    /// <remarks>
    /// Roles requeridos: ninguno (endpoint público).
    /// Códigos de respuesta:
    /// - 200: registro exitoso.
    /// - 400: validación o regla de negocio (vía middleware).
    /// - 500: error interno (vía middleware).
    /// </remarks>
    [HttpPost("register")]
    [ProducesResponseType<AuthResultDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResultDto>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await registerHandler.HandleAsync(
            new RegisterCommand(
                NombreCompleto: request.NombreCompleto,
                Email: request.Email,
                Password: request.Password,
                Rol: request.Rol),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Autentica un usuario existente y emite un token JWT.
    /// </summary>
    /// <param name="request">Credenciales (correo y contraseña).</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resultado de autenticación con token JWT, o 401 si las credenciales fallan.</returns>
    /// <remarks>
    /// Roles requeridos: ninguno (endpoint público).
    /// Códigos de respuesta:
    /// - 200: credenciales válidas; cuerpo con token y datos del usuario.
    /// - 401: correo o contraseña incorrectos.
    /// - 400: validación de entrada (vía middleware).
    /// - 500: error interno (vía middleware).
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType<AuthResultDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResultDto>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await loginHandler.HandleAsync(
                new LoginCommand(request.Email, request.Password),
                cancellationToken);

            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Credenciales inválidas",
                Detail = "El correo o la contraseña no son correctos.",
                Status = StatusCodes.Status401Unauthorized
            });
        }
    }
}
