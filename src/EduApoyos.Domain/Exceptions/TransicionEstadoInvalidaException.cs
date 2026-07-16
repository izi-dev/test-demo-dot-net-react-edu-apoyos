using EduApoyos.Domain.Enums;

namespace EduApoyos.Domain.Exceptions;

/// <summary>
/// Indica que una solicitud intentó cambiar a un estado no permitido por el flujo de negocio.
/// </summary>
public sealed class TransicionEstadoInvalidaException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia con el estado actual y el estado destino inválido.
    /// </summary>
    /// <param name="estadoActual">Estado en el que se encuentra la solicitud.</param>
    /// <param name="estadoDestino">Estado al que se intentó transicionar sin éxito.</param>
    public TransicionEstadoInvalidaException(EstadoSolicitud estadoActual, EstadoSolicitud estadoDestino)
        : base($"No es posible cambiar de {estadoActual} a {estadoDestino}.")
    {
        EstadoActual = estadoActual;
        EstadoDestino = estadoDestino;
    }

    /// <summary>
    /// Estado actual de la solicitud al momento del intento fallido.
    /// </summary>
    public EstadoSolicitud EstadoActual { get; }

    /// <summary>
    /// Estado destino que se intentó aplicar y no está permitido.
    /// </summary>
    public EstadoSolicitud EstadoDestino { get; }
}
