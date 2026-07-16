using System.Security.Claims;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Api.Auth;

/// <summary>
/// Utilidades para leer información del usuario autenticado desde el token JWT.
/// </summary>
public static class UserClaimsExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("Token sin identificador de usuario.");
    }

    public static RolUsuario GetRol(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.Role);
        return Enum.TryParse<RolUsuario>(value, out var rol)
            ? rol
            : throw new UnauthorizedAccessException("Token sin rol válido.");
    }
}
