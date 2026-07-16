namespace EduApoyos.Domain.Exceptions;

/// <summary>
/// Indica que un recurso solicitado no existe en el dominio.
/// </summary>
public sealed class RecursoNoEncontradoException : DomainException
{
    public RecursoNoEncontradoException(string nombreRecurso, Guid id)
        : base($"No se encontró {nombreRecurso} con identificador '{id}'.")
    {
        NombreRecurso = nombreRecurso;
        RecursoId = id;
    }

    public string NombreRecurso { get; }
    public Guid RecursoId { get; }
}
