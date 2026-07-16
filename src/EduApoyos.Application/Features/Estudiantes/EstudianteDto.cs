using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.Features.Estudiantes;

/// <summary>
/// Proyección de lectura de un estudiante.
/// </summary>
public sealed record EstudianteDto(
    Guid Id,
    Guid UsuarioId,
    string NombreCompleto,
    string Email,
    string NumeroDocumento,
    TipoDocumento TipoDocumento,
    string ProgramaAcademico,
    int Semestre);
