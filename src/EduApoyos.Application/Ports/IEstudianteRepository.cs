using EduApoyos.Application.Common;
using EduApoyos.Domain.Entities;

namespace EduApoyos.Application.Ports;

/// <summary>
/// Puerto de lectura y escritura de estudiantes orientado a casos de uso.
/// </summary>
/// <remarks>
/// Permite resolver el perfil académico a partir del usuario autenticado,
/// listar estudiantes para el panel del asesor y crear nuevos perfiles.
/// </remarks>
public interface IEstudianteRepository
{
    /// <summary>
    /// Obtiene un estudiante por su identificador de perfil.
    /// </summary>
    /// <param name="id">Identificador del estudiante.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>El estudiante si existe; de lo contrario <c>null</c>.</returns>
    /// <remarks>
    /// Usado al crear solicitudes por un asesor que indica explícitamente el estudiante destino.
    /// </remarks>
    Task<Estudiante?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el estudiante asociado a un usuario de identidad.
    /// </summary>
    /// <param name="usuarioId">Identificador del usuario dueño del perfil de estudiante.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>El estudiante vinculado; o <c>null</c> si el usuario aún no tiene perfil.</returns>
    /// <remarks>
    /// Es la resolución típica en flujos del portal del estudiante (listar mis solicitudes,
    /// crear solicitud propia, generar constancia, autorización por recurso).
    /// </remarks>
    Task<Estudiante?> ObtenerPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista estudiantes de forma paginada.
    /// </summary>
    /// <param name="pagina">Número de página (base 1).</param>
    /// <param name="tamanoPagina">Cantidad de elementos por página.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Resultado paginado con estudiantes y metadatos de paginación.</returns>
    /// <remarks>
    /// Pensado para el panel del asesor. La implementación debe incluir datos de usuario
    /// (nombre/email) necesarios para proyectar <c>EstudianteDto</c>.
    /// </remarks>
    Task<PagedResult<Estudiante>> ListarAsync(int pagina, int tamanoPagina, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca un nuevo estudiante para inserción en el almacén de persistencia.
    /// </summary>
    /// <param name="estudiante">Agregado de estudiante creado por el dominio.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Una tarea que representa el encolado de la inserción.</returns>
    /// <remarks>
    /// No confirma la transacción; requiere <see cref="IUnitOfWork.SaveChangesAsync"/>.
    /// </remarks>
    Task AgregarAsync(Estudiante estudiante, CancellationToken cancellationToken = default);

    /// <summary>
    /// Indica si ya existe un perfil de estudiante para el usuario dado.
    /// </summary>
    /// <param name="usuarioId">Identificador del usuario a comprobar.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns><c>true</c> si existe un estudiante con ese <paramref name="usuarioId"/>; en caso contrario <c>false</c>.</returns>
    /// <remarks>
    /// Útil para validaciones de unicidad antes de crear el perfil académico.
    /// </remarks>
    Task<bool> ExisteUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
