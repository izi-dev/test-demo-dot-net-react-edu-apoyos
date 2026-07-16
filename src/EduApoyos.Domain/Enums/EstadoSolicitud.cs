namespace EduApoyos.Domain.Enums;

/// <summary>
/// Estados posibles del ciclo de vida de una solicitud de apoyo económico.
/// </summary>
public enum EstadoSolicitud
{
    /// <summary>
    /// Solicitud registrada y pendiente de revisión por un asesor.
    /// </summary>
    Pendiente = 1,

    /// <summary>
    /// Solicitud en proceso de evaluación por un asesor.
    /// </summary>
    EnRevision = 2,

    /// <summary>
    /// Solicitud aprobada; el apoyo económico fue concedido.
    /// </summary>
    Aprobada = 3,

    /// <summary>
    /// Solicitud rechazada; el apoyo económico no fue concedido.
    /// </summary>
    Rechazada = 4
}
