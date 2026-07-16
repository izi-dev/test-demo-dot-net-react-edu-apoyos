namespace EduApoyos.Domain.Exceptions;

/// <summary>
/// Indica que un recurso solicitado no existe en el dominio.
/// </summary>
public sealed class RecursoNoEncontradoException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia con el nombre del recurso y su identificador.
    /// </summary>
    /// <param name="nombreRecurso">Nombre descriptivo del tipo de recurso buscado (por ejemplo, "Solicitud").</param>
    /// <param name="id">Identificador del recurso que no fue encontrado.</param>
    public RecursoNoEncontradoException(string nombreRecurso, Guid id)
        : base($"No se encontró {nombreRecurso} con identificador '{id}'.")
    {
        NombreRecurso = nombreRecurso;
        RecursoId = id;
    }

    /// <summary>
    /// Nombre descriptivo del tipo de recurso que no se encontró.
    /// </summary>
    public string NombreRecurso { get; }

    /// <summary>
    /// Identificador del recurso que no existe.
    /// </summary>
    public Guid RecursoId { get; }
}
