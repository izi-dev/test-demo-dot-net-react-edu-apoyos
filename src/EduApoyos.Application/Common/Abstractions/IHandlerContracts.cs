namespace EduApoyos.Application.Common.Abstractions;

/// <summary>
/// Contrato para manejadores de comandos que modifican el estado del sistema.
/// </summary>
public interface ICommandHandler<in TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contrato para manejadores de consultas de solo lectura.
/// </summary>
public interface IQueryHandler<in TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
