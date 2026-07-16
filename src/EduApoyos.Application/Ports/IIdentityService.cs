using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.Ports;

/// <summary>
/// Puerto de autenticación e identidad. Abstrae ASP.NET Core Identity.
/// </summary>
public interface IIdentityService
{
    Task<RegistroUsuarioResultado> RegistrarAsync(RegistroUsuarioSolicitud solicitud, CancellationToken cancellationToken = default);
    Task<UsuarioAutenticado?> AutenticarAsync(string email, string password, CancellationToken cancellationToken = default);
}

/// <summary>
/// Datos necesarios para registrar un usuario en el sistema.
/// </summary>
public sealed record RegistroUsuarioSolicitud(
    string NombreCompleto,
    string Email,
    string Password,
    RolUsuario Rol);

/// <summary>
/// Resultado del proceso de registro de usuario.
/// </summary>
public sealed record RegistroUsuarioResultado(
    bool Exitoso,
    UsuarioAutenticado? Usuario,
    IReadOnlyDictionary<string, string[]>? Errores);

/// <summary>
/// Representa un usuario autenticado en el dominio de aplicación.
/// </summary>
public sealed record UsuarioAutenticado(
    Guid Id,
    string NombreCompleto,
    string Email,
    RolUsuario Rol);
