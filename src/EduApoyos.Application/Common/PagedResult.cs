namespace EduApoyos.Application.Common;

/// <summary>
/// Resultado paginado de una consulta con metadatos de paginación.
/// </summary>
/// <typeparam name="T">Tipo de los elementos contenidos en la página.</typeparam>
/// <param name="Items">Colección de elementos de la página actual.</param>
/// <param name="Page">Número de página solicitada (base 1).</param>
/// <param name="PageSize">Cantidad de elementos por página.</param>
/// <param name="TotalItems">Total de elementos que cumplen el criterio de búsqueda.</param>
public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalItems)
{
    /// <summary>
    /// Número total de páginas calculado a partir del total de elementos y el tamaño de página.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
}
