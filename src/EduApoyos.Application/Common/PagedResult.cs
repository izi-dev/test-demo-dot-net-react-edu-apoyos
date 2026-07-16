namespace EduApoyos.Application.Common;

/// <summary>
/// Resultado paginado de una consulta con metadatos de paginación.
/// </summary>
/// <typeparam name="T">Tipo de los elementos contenidos en la página.</typeparam>
/// <param name="Items">Colección de elementos de la página actual.</param>
/// <param name="Page">Número de página solicitada (base 1).</param>
/// <param name="PageSize">Cantidad de elementos por página.</param>
/// <param name="TotalItems">Total de elementos que cumplen el criterio de búsqueda.</param>
/// <remarks>
/// Se usa como contrato de retorno de listados de aplicación (estudiantes, solicitudes, etc.).
/// <see cref="TotalPages"/> se calcula a partir de <paramref name="TotalItems"/> y <paramref name="PageSize"/>.
/// </remarks>
public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalItems)
{
    /// <summary>
    /// Número total de páginas calculado a partir del total de elementos y el tamaño de página.
    /// </summary>
    /// <remarks>
    /// Utiliza redondeo hacia arriba (<c>Math.Ceiling</c>). Si <see cref="PageSize"/> es cero,
    /// el resultado puede ser indefinido o lanzar división por cero según el runtime;
    /// los handlers normalizan el tamaño de página antes de construir este resultado.
    /// </remarks>
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
}
