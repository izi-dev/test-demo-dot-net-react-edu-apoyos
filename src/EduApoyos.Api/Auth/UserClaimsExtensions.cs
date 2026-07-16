using System.Security.Claims;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Api.Auth;

/// <summary>
/// Extensiones para leer claims del usuario autenticado desde el token JWT.
/// </summary>
public static class UserClaimsExtensions
{
    /// <summary>
    /// Obtiene el identificador de usuario desde <see cref="ClaimTypes.NameIdentifier"/>.
    /// </summary>
    /// <param name="user">Principal de la solicitud autenticada.</param>
    /// <returns>GUID del usuario autenticado.</returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Se lanza si el claim no existe o no es un GUID válido.
    /// </exception>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("Token sin identificador de usuario.");
    }

    /// <summary>
    /// Obtiene el rol de negocio desde <see cref="ClaimTypes.Role"/>.
    /// </summary>
    /// <param name="user">Principal de la solicitud autenticada.</param>
    /// <returns>Rol de dominio (<see cref="RolUsuario"/>).</returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Se lanza si el claim no existe o no corresponde a un valor de <see cref="RolUsuario"/>.
    /// </exception>
    public static RolUsuario GetRol(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.Role);
        return Enum.TryParse<RolUsuario>(value, out var rol)
            ? rol
            : throw new UnauthorizedAccessException("Token sin rol válido.");
    }
}
