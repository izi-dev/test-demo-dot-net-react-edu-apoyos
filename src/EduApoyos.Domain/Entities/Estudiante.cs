using EduApoyos.Domain.Enums;

namespace EduApoyos.Domain.Entities;

/// <summary>
/// Representa el perfil académico de un estudiante vinculado a un usuario del sistema.
/// </summary>
public class Estudiante
{
    /// <summary>
    /// Identificador único del estudiante.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador del usuario al que pertenece este perfil de estudiante.
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Navegación al usuario asociado al perfil de estudiante.
    /// </summary>
    public Usuario? Usuario { get; set; }

    /// <summary>
    /// Número del documento de identificación del estudiante.
    /// </summary>
    public string NumeroDocumento { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de documento de identificación del estudiante.
    /// </summary>
    public TipoDocumento TipoDocumento { get; set; }

    /// <summary>
    /// Nombre del programa académico en el que está inscrito el estudiante.
    /// </summary>
    public string ProgramaAcademico { get; set; } = string.Empty;

    /// <summary>
    /// Semestre académico actual del estudiante (entre 1 y 15).
    /// </summary>
    public int Semestre { get; set; }

    /// <summary>
    /// Colección de solicitudes de apoyo asociadas al estudiante.
    /// </summary>
    public ICollection<SolicitudApoyo> Solicitudes { get; private set; } = new List<SolicitudApoyo>();

    /// <summary>
    /// Crea un estudiante validando datos mínimos del perfil académico.
    /// </summary>
    /// <param name="usuarioId">Identificador del usuario vinculado; no puede ser <see cref="Guid.Empty"/>.</param>
    /// <param name="usuario">Entidad de usuario asociada al perfil.</param>
    /// <param name="numeroDocumento">Número de documento; no puede ser nulo ni vacío (se recorta con <c>Trim</c>).</param>
    /// <param name="tipoDocumento">Tipo de documento de identificación.</param>
    /// <param name="programaAcademico">Programa académico; no puede ser nulo ni vacío (se recorta con <c>Trim</c>).</param>
    /// <param name="semestre">Semestre académico; debe estar entre 1 y 15 inclusive.</param>
    /// <returns>Una instancia de <see cref="Estudiante"/> con un nuevo <see cref="Id"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Se lanza cuando <paramref name="usuarioId"/> es vacío, o cuando
    /// <paramref name="numeroDocumento"/> o <paramref name="programaAcademico"/> están vacíos o solo contienen espacios.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Se lanza cuando <paramref name="semestre"/> es menor que 1 o mayor que 15.
    /// </exception>
    /// <remarks>
    /// Validaciones y reglas aplicadas:
    /// <list type="bullet">
    /// <item><description><paramref name="usuarioId"/> no puede ser <see cref="Guid.Empty"/>.</description></item>
    /// <item><description><paramref name="numeroDocumento"/> no puede ser nulo, vacío ni solo espacios en blanco.</description></item>
    /// <item><description><paramref name="programaAcademico"/> no puede ser nulo, vacío ni solo espacios en blanco.</description></item>
    /// <item><description><paramref name="semestre"/> debe estar en el rango inclusivo [1, 15].</description></item>
    /// <item><description>Al crear, <see cref="NumeroDocumento"/> y <see cref="ProgramaAcademico"/> se almacenan recortados (<c>Trim</c>).</description></item>
    /// <item><description>Se genera un nuevo <see cref="Id"/> con <see cref="Guid.NewGuid"/>.</description></item>
    /// </list>
    /// </remarks>
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
