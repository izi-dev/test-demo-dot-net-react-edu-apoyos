using EduApoyos.Domain.Enums;

namespace EduApoyos.Domain.Entities;

/// <summary>
/// Representa el perfil académico de un estudiante vinculado a un usuario del sistema.
/// </summary>
public class Estudiante
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public TipoDocumento TipoDocumento { get; set; }
    public string ProgramaAcademico { get; set; } = string.Empty;
    public int Semestre { get; set; }
    public ICollection<SolicitudApoyo> Solicitudes { get; private set; } = new List<SolicitudApoyo>();

    /// <summary>
    /// Crea un estudiante validando datos mínimos del perfil académico.
    /// </summary>
    public static Estudiante Crear(
        Guid usuarioId,
        Usuario usuario,
        string numeroDocumento,
        TipoDocumento tipoDocumento,
        string programaAcademico,
        int semestre)
    {
        if (usuarioId == Guid.Empty)
        {
            throw new ArgumentException("El usuario es obligatorio.", nameof(usuarioId));
        }

        if (string.IsNullOrWhiteSpace(numeroDocumento))
        {
            throw new ArgumentException("El número de documento es obligatorio.", nameof(numeroDocumento));
        }

        if (string.IsNullOrWhiteSpace(programaAcademico))
        {
            throw new ArgumentException("El programa académico es obligatorio.", nameof(programaAcademico));
        }

        if (semestre is < 1 or > 15)
        {
            throw new ArgumentOutOfRangeException(nameof(semestre), "El semestre debe estar entre 1 y 15.");
        }

        return new Estudiante
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Usuario = usuario,
            NumeroDocumento = numeroDocumento.Trim(),
            TipoDocumento = tipoDocumento,
            ProgramaAcademico = programaAcademico.Trim(),
            Semestre = semestre
        };
    }
}
