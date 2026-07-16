namespace EduApoyos.Domain.Enums;

/// <summary>
/// Roles de usuario reconocidos por el sistema de apoyos educativos.
/// </summary>
public enum RolUsuario
{
    /// <summary>
    /// Usuario con permisos para revisar, aprobar o rechazar solicitudes.
    /// </summary>
    Asesor = 1,

    /// <summary>
    /// Usuario que puede crear y consultar sus propias solicitudes de apoyo.
    /// </summary>
    Estudiante = 2
}
