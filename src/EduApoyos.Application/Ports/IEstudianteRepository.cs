using EduApoyos.Application.Common;
using EduApoyos.Domain.Entities;

namespace EduApoyos.Application.Ports;

/// <summary>
/// Puerto de lectura y escritura de estudiantes orientado a casos de uso.
/// </summary>
public interface IEstudianteRepository
{
    Task<Estudiante?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Estudiante?> ObtenerPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<PagedResult<Estudiante>> ListarAsync(int pagina, int tamanoPagina, CancellationToken cancellationToken = default);
    Task AgregarAsync(Estudiante estudiante, CancellationToken cancellationToken = default);
    Task<bool> ExisteUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
