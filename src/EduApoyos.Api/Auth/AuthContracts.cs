using EduApoyos.Domain.Enums;

namespace EduApoyos.Api.Auth;

public sealed record RegisterRequest(
    string NombreCompleto,
    string Email,
    string Password,
    RolUsuario Rol);

public sealed record LoginRequest(string Email, string Password);
