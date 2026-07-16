using EduApoyos.Domain.Enums;

namespace EduApoyos.Api.Auth;

/// <summary>
/// Contrato HTTP del cuerpo de registro de un usuario.
/// </summary>
/// <param name="NombreCompleto">Nombre completo del nuevo usuario.</param>
/// <param name="Email">Correo electrónico (también usado como nombre de usuario).</param>
/// <param name="Password">Contraseña en texto plano (validada por Identity).</param>
/// <param name="Rol">Rol de negocio a asignar (<see cref="RolUsuario"/>).</param>
public sealed record RegisterRequest(
    string NombreCompleto,
    string Email,
    string Password,
    RolUsuario Rol);

/// <summary>
/// Contrato HTTP del cuerpo de inicio de sesión.
/// </summary>
/// <param name="Email">Correo electrónico del usuario.</param>
/// <param name="Password">Contraseña en texto plano.</param>
public sealed record LoginRequest(string Email, string Password);
