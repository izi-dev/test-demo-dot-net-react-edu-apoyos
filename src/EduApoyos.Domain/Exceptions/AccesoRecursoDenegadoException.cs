namespace EduApoyos.Domain.Exceptions;

/// <summary>
/// Indica que el usuario autenticado no tiene permiso sobre el recurso solicitado.
/// </summary>
public sealed class AccesoRecursoDenegadoException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia con el mensaje de denegación de acceso.
    /// </summary>
    /// <param name="mensaje">Descripción del motivo por el cual se denegó el acceso al recurso.</param>
    public AccesoRecursoDenegadoException(string mensaje) : base(mensaje)
    {
    }
}
