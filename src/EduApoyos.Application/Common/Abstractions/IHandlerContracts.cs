namespace EduApoyos.Application.Common.Abstractions;

/// <summary>
/// Contrato para manejadores de comandos que modifican el estado del sistema.
/// </summary>
/// <typeparam name="TCommand">Tipo del comando de entrada.</typeparam>
/// <typeparam name="TResult">Tipo del resultado producido por el handler.</typeparam>
/// <remarks>
/// Define el patrón de vertical slice usado en Auth, Estudiantes y Solicitudes:
/// el handler recibe un record de comando y devuelve un DTO o valor de aplicación.
/// </remarks>
public interface ICommandHandler<in TCommand, TResult>
{
    /// <summary>
    /// Ejecuta el caso de uso asociado al comando.
    /// </summary>
    /// <param name="command">Comando con los datos de la operación.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Resultado del comando (típicamente un DTO).</returns>
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contrato para manejadores de consultas de solo lectura.
/// </summary>
/// <typeparam name="TQuery">Tipo de la consulta de entrada.</typeparam>
/// <typeparam name="TResult">Tipo del resultado de lectura.</typeparam>
/// <remarks>
/// Las consultas no deben mutar el estado del dominio; pueden aplicar autorización
/// por recurso y proyección a DTOs.
/// </remarks>
public interface IQueryHandler<in TQuery, TResult>
{
    /// <summary>
    /// Ejecuta la consulta y proyecta el resultado.
    /// </summary>
    /// <param name="query">Consulta con criterios de búsqueda o identificadores.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Resultado de la consulta (DTO, colección o resultado paginado).</returns>
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
