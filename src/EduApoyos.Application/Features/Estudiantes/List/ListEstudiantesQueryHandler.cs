using EduApoyos.Application.Common;
using EduApoyos.Application.Common.Abstractions;
using EduApoyos.Application.Ports;

namespace EduApoyos.Application.Features.Estudiantes.List;

/// <summary>
/// Consulta paginada de estudiantes para el panel del asesor.
/// </summary>
/// <param name="Pagina">Número de página (base 1). Valores menores a 1 se normalizan a 1.</param>
/// <param name="TamanoPagina">
/// Tamaño de página. Valores menores a 1 o mayores a 100 se normalizan a 10.
/// </param>
public sealed record ListEstudiantesQuery(int Pagina = 1, int TamanoPagina = 10);

/// <summary>
/// Lista estudiantes con paginación.
/// </summary>
/// <remarks>
/// No tiene validator FluentValidation; normaliza página y tamaño en el propio handler.
/// </remarks>
/// <param name="estudiantes">Puerto de lectura de estudiantes.</param>
public sealed class ListEstudiantesQueryHandler(
    IEstudianteRepository estudiantes) : IQueryHandler<ListEstudiantesQuery, PagedResult<EstudianteDto>>
{
    /// <summary>
    /// Obtiene una página de estudiantes proyectada a <see cref="EstudianteDto"/>.
    /// </summary>
    /// <param name="query">Criterios de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>
    /// <see cref="PagedResult{T}"/> de <see cref="EstudianteDto"/> con metadatos de paginación.
    /// </returns>
    /// <remarks>
    /// No lanza excepciones de dominio específicas: si no hay datos, retorna página vacía.
    /// </remarks>
    public async Task<PagedResult<EstudianteDto>> HandleAsync(ListEstudiantesQuery query, CancellationToken cancellationToken = default)
    {
        var pagina = query.Pagina < 1 ? 1 : query.Pagina;
        var tamano = query.TamanoPagina is < 1 or > 100 ? 10 : query.TamanoPagina;

        var resultado = await estudiantes.ListarAsync(pagina, tamano, cancellationToken);

        return new PagedResult<EstudianteDto>(
            Items: resultado.Items.Select(x => new EstudianteDto(
                Id: x.Id,
                UsuarioId: x.UsuarioId,
                NombreCompleto: x.Usuario?.NombreCompleto ?? string.Empty,
                Email: x.Usuario?.Email ?? string.Empty,
                NumeroDocumento: x.NumeroDocumento,
                TipoDocumento: x.TipoDocumento,
                ProgramaAcademico: x.ProgramaAcademico,
                Semestre: x.Semestre)).ToArray(),
            Page: resultado.Page,
            PageSize: resultado.PageSize,
            TotalItems: resultado.TotalItems);
    }
}
