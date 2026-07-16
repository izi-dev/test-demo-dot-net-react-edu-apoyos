using EduApoyos.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EduApoyos.Infrastructure.Identity;

/// <summary>
/// Usuario de autenticación extendido con datos de dominio para EduApoyos.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Rol asignado al usuario en el sistema de apoyos.
    /// </summary>
    public RolUsuario Rol { get; set; }

    /// <summary>
    /// Fecha y hora UTC en que el usuario fue registrado.
    /// </summary>
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
