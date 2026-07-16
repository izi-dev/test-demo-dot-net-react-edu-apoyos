namespace EduApoyos.Domain.Exceptions;

/// <summary>
/// Excepción base para violaciones de reglas de negocio del dominio.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Inicializa una nueva instancia de la excepción de dominio con el mensaje indicado.
    /// </summary>
    /// <param name="message">Mensaje que describe la violación de la regla de negocio.</param>
    protected DomainException(string message) : base(message)
    {
    }
}
