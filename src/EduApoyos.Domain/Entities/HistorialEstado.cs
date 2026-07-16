using EduApoyos.Domain.Enums;

namespace EduApoyos.Domain.Entities;

/// <summary>
/// Registro de auditoría de un cambio de estado en una solicitud de apoyo.
/// </summary>
public class HistorialEstado
{
    /// <summary>
    /// Identificador único del registro de historial.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador de la solicitud afectada por el cambio.
    /// </summary>
    public Guid SolicitudId { get; set; }

    /// <summary>
    /// Solicitud asociada al registro de historial.
    /// </summary>
    public SolicitudApoyo? Solicitud { get; set; }

    /// <summary>
    /// Estado anterior de la solicitud; nulo cuando la solicitud se crea por primera vez.
    /// </summary>
    public EstadoSolicitud? EstadoAnterior { get; set; }

    /// <summary>
    /// Nuevo estado asignado a la solicitud.
    /// </summary>
    public EstadoSolicitud EstadoNuevo { get; set; }

    /// <summary>
    /// Fecha y hora UTC en que se realizó el cambio de estado.
    /// </summary>
    public DateTime FechaCambio { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Identificador del usuario que ejecutó el cambio de estado.
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Usuario que ejecutó el cambio de estado.
    /// </summary>
    public Usuario? Usuario { get; set; }

    /// <summary>
    /// Observación o comentario asociado al cambio de estado.
    /// </summary>
    public string Observacion { get; set; } = string.Empty;
}
