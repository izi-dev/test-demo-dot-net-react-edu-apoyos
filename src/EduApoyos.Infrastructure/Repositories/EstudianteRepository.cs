using EduApoyos.Application.Common;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Entities;
using EduApoyos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Infrastructure.Repositories;

/// <summary>
/// Implementación EF Core del puerto <see cref="IEstudianteRepository"/>.
/// Incluye la navegación a <see cref="Usuario"/> en las consultas de lectura.
/// </summary>
public sealed class EstudianteRepository(EduApoyosDbContext context) : IEstudianteRepository
{
    /// <summary>
    /// Obtiene un estudiante por su identificador, con el usuario asociado.
    /// </summary>
    /// <param name="id">Identificador del estudiante.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>El estudiante si existe; de lo contrario, <c>null</c>.</returns>
    public Task<Estudiante?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    /// <summary>
    /// Obtiene el perfil de estudiante vinculado a un usuario de dominio.
    /// </summary>
    /// <param name="usuarioId">Identificador del usuario.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>El estudiante si existe; de lo contrario, <c>null</c>.</returns>
    public Task<Estudiante?> ObtenerPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default) =>
        Query().FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

    /// <summary>
    /// Lista estudiantes paginados ordenados por nombre completo del usuario.
    /// </summary>
    /// <param name="pagina">Número de página (1-based).</param>
    /// <param name="tamanoPagina">Cantidad de elementos por página.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resultado paginado de estudiantes.</returns>
    public async Task<PagedResult<Estudiante>> ListarAsync(int pagina, int tamanoPagina, CancellationToken cancellationToken = default)
    {
        var query = Query().OrderBy(x => x.Usuario!.NombreCompleto);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina).ToArrayAsync(cancellationToken);
        return new PagedResult<Estudiante>(items, pagina, tamanoPagina, total);
    }

    /// <summary>
    /// Agrega un estudiante al contexto (pendiente de <c>SaveChanges</c>).
    /// </summary>
    /// <param name="estudiante">Entidad de estudiante a insertar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Una tarea que completa cuando la entidad queda rastreada para inserción.</returns>
    public async Task AgregarAsync(Estudiante estudiante, CancellationToken cancellationToken = default) =>
        await context.Estudiantes.AddAsync(estudiante, cancellationToken);

    /// <summary>
    /// Indica si ya existe un estudiante asociado al usuario indicado.
    /// </summary>
    /// <param name="usuarioId">Identificador del usuario.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns><c>true</c> si existe; de lo contrario, <c>false</c>.</returns>
    public Task<bool> ExisteUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default) =>
        context.Estudiantes.AnyAsync(x => x.UsuarioId == usuarioId, cancellationToken);

    /// <summary>
    /// Consulta base de estudiantes con include de usuario.
    /// </summary>
    /// <returns>Consulta IQueryable lista para filtrar o materializar.</returns>
    private IQueryable<Estudiante> Query() =>
        context.Estudiantes.Include(x => x.Usuario);
}
