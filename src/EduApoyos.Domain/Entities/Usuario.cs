using EduApoyos.Domain.Enums;

namespace EduApoyos.Domain.Entities;

/// <summary>
/// Representa un usuario del dominio con credenciales y rol asignado.
/// </summary>
public class Usuario
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico único del usuario.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hash de la contraseña almacenado de forma segura.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Rol que determina los permisos del usuario en el sistema.
    /// </summary>
    public RolUsuario Rol { get; set; }

    /// <summary>
    /// Fecha y hora UTC en que el usuario fue registrado.
    /// </summary>
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
