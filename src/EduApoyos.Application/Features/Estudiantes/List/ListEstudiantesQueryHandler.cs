using EduApoyos.Application.Common;
using EduApoyos.Application.Common.Abstractions;
using EduApoyos.Application.Ports;

namespace EduApoyos.Application.Features.Estudiantes.List;

/// <summary>
/// Consulta paginada de estudiantes para el panel del asesor.
/// </summary>
public sealed record ListEstudiantesQuery(int Pagina = 1, int TamanoPagina = 10);

/// <summary>
/// Lista estudiantes con paginación.
/// </summary>
public sealed class ListEstudiantesQueryHandler(
    IEstudianteRepository estudiantes) : IQueryHandler<ListEstudiantesQuery, PagedResult<EstudianteDto>>
{
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
