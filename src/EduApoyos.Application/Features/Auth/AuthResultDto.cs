using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.Features.Auth;

/// <summary>
/// Respuesta de autenticación entregada al cliente tras login o registro.
/// </summary>
/// <param name="Token">JWT compacto listo para usarse en el encabezado Authorization.</param>
/// <param name="ExpiresAt">Fecha/hora en la que el token expira.</param>
/// <param name="UserId">Identificador del usuario autenticado.</param>
/// <param name="NombreCompleto">Nombre completo del usuario.</param>
/// <param name="Email">Correo electrónico del usuario.</param>
/// <param name="Rol">Rol de dominio del usuario.</param>
/// <remarks>
/// Producido por <c>LoginCommandHandler</c> y <c>RegisterCommandHandler</c>.
/// </remarks>
public sealed record AuthResultDto(
    string Token,
    DateTime ExpiresAt,
    Guid UserId,
    string NombreCompleto,
    string Email,
    RolUsuario Rol);
