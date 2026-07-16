namespace EduApoyos.Domain.Exceptions;

/// <summary>
/// Excepción base para violaciones de reglas de negocio del dominio.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
