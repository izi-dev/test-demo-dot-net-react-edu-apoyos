using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.Features.Auth;

/// <summary>
/// Respuesta de autenticación entregada al cliente tras login o registro.
/// </summary>
public sealed record AuthResultDto(
    string Token,
    DateTime ExpiresAt,
    Guid UserId,
    string NombreCompleto,
    string Email,
    RolUsuario Rol);
