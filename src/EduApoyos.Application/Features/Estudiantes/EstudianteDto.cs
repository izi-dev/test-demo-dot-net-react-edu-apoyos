using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.Features.Estudiantes;

/// <summary>
/// Proyección de lectura de un estudiante.
/// </summary>
/// <param name="Id">Identificador del perfil de estudiante.</param>
/// <param name="UsuarioId">Identificador del usuario de identidad asociado.</param>
/// <param name="NombreCompleto">Nombre completo tomado del usuario vinculado.</param>
/// <param name="Email">Correo del usuario vinculado.</param>
/// <param name="NumeroDocumento">Número de documento de identidad académica.</param>
/// <param name="TipoDocumento">Tipo de documento (CC, TI, etc.).</param>
/// <param name="ProgramaAcademico">Programa o carrera del estudiante.</param>
/// <param name="Semestre">Semestre académico actual (1–15).</param>
public sealed record EstudianteDto(
    Guid Id,
    Guid UsuarioId,
    string NombreCompleto,
    string Email,
    string NumeroDocumento,
    TipoDocumento TipoDocumento,
    string ProgramaAcademico,
    int Semestre);
