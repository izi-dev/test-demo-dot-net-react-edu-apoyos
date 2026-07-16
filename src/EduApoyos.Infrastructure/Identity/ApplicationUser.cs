using EduApoyos.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EduApoyos.Infrastructure.Identity;

/// <summary>
/// Usuario de ASP.NET Core Identity extendido con datos de dominio de EduApoyos.
/// Comparte el mismo <see cref="IdentityUser{TKey}.Id"/> que la entidad de dominio <c>Usuario</c>.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Rol de negocio asignado al usuario (Asesor o Estudiante).
    /// </summary>
    public RolUsuario Rol { get; set; }

    /// <summary>
    /// Fecha y hora UTC en que el usuario fue registrado.
    /// </summary>
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
