namespace EduApoyos.Domain.Exceptions;

/// <summary>
/// Indica que el usuario autenticado no tiene permiso sobre el recurso solicitado.
/// </summary>
public sealed class AccesoRecursoDenegadoException : DomainException
{
    public AccesoRecursoDenegadoException(string mensaje) : base(mensaje)
    {
    }
}
