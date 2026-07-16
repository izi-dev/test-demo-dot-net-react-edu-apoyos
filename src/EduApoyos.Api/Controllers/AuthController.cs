using EduApoyos.Api.Auth;
using EduApoyos.Application.Features.Auth;
using EduApoyos.Application.Features.Auth.Login;
using EduApoyos.Application.Features.Auth.Register;
using Microsoft.AspNetCore.Mvc;

namespace EduApoyos.Api.Controllers;

/// <summary>
/// Controlador delgado de autenticación. Solo traduce HTTP a casos de uso.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    RegisterCommandHandler registerHandler,
    LoginCommandHandler loginHandler) : ControllerBase
{
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
