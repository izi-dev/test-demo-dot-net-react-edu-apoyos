using EduApoyos.Domain.Enums;

namespace EduApoyos.Domain.Exceptions;

/// <summary>
/// Indica que una solicitud intentó cambiar a un estado no permitido por el flujo de negocio.
/// </summary>
public sealed class TransicionEstadoInvalidaException : DomainException
{
    public TransicionEstadoInvalidaException(EstadoSolicitud estadoActual, EstadoSolicitud estadoDestino)
        : base($"No es posible cambiar de {estadoActual} a {estadoDestino}.")
    {
        EstadoActual = estadoActual;
        EstadoDestino = estadoDestino;
    }

    public EstadoSolicitud EstadoActual { get; }
    public EstadoSolicitud EstadoDestino { get; }
}
