using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Exceptions;
using EduApoyos.Domain.ValueObjects;

namespace EduApoyos.Domain.Entities;

/// <summary>
/// Agregado raíz que representa una solicitud de apoyo económico y su ciclo de vida.
/// </summary>
public class SolicitudApoyo
{
    private SolicitudApoyo()
    {
    }

    public Guid Id { get; private set; }
    public Guid EstudianteId { get; private set; }
    public Estudiante? Estudiante { get; private set; }
    public TipoApoyo TipoApoyo { get; private set; }
    public decimal MontoSolicitado { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public EstadoSolicitud Estado { get; private set; } = EstadoSolicitud.Pendiente;
    public DateTime FechaSolicitud { get; private set; }
    public DateTime FechaActualizacion { get; private set; }
    public Guid? AsesorId { get; private set; }
    public Usuario? Asesor { get; private set; }
    public ICollection<HistorialEstado> Historial { get; private set; } = new List<HistorialEstado>();

    /// <summary>
    /// Crea una nueva solicitud en estado <see cref="EstadoSolicitud.Pendiente"/>.
    /// </summary>
    public static SolicitudApoyo Crear(
        Guid estudianteId,
        Estudiante estudiante,
        TipoApoyo tipoApoyo,
        MontoSolicitado monto,
        DescripcionSolicitud descripcion,
        Guid usuarioCreadorId)
    {
        var ahora = DateTime.UtcNow;
        var solicitud = new SolicitudApoyo
        {
            Id = Guid.NewGuid(),
            EstudianteId = estudianteId,
            Estudiante = estudiante,
            TipoApoyo = tipoApoyo,
            MontoSolicitado = monto.Valor,
            Descripcion = descripcion.Valor,
            Estado = EstadoSolicitud.Pendiente,
            FechaSolicitud = ahora,
            FechaActualizacion = ahora
        };

        solicitud.RegistrarHistorial(
            estadoAnterior: null,
            estadoNuevo: EstadoSolicitud.Pendiente,
            usuarioId: usuarioCreadorId,
            observacion: "Solicitud creada.");

        return solicitud;
    }

    /// <summary>
    /// Cambia el estado validando las transiciones permitidas del flujo institucional.
    /// </summary>
    public void CambiarEstado(EstadoSolicitud nuevoEstado, Guid usuarioId, string? observacion, Guid? asesorId = null)
    {
        if (!PuedeCambiarA(nuevoEstado))
        {
            throw new TransicionEstadoInvalidaException(Estado, nuevoEstado);
        }

        var estadoAnterior = Estado;
        Estado = nuevoEstado;
        FechaActualizacion = DateTime.UtcNow;
        AsesorId = asesorId ?? AsesorId;

        RegistrarHistorial(
            estadoAnterior: estadoAnterior,
            estadoNuevo: nuevoEstado,
            usuarioId: usuarioId,
            observacion: observacion ?? string.Empty);
    }

    /// <summary>
    /// Evalúa si la transición al estado destino está permitida.
    /// </summary>
    public bool PuedeCambiarA(EstadoSolicitud nuevoEstado) =>
        Estado switch
        {
            EstadoSolicitud.Pendiente => nuevoEstado == EstadoSolicitud.EnRevision,
            EstadoSolicitud.EnRevision => nuevoEstado is EstadoSolicitud.Aprobada or EstadoSolicitud.Rechazada,
            _ => false
        };

    /// <summary>
    /// Indica si la solicitud pertenece al estudiante indicado.
    /// </summary>
    public bool PerteneceA(Guid estudianteId) => EstudianteId == estudianteId;

    private void RegistrarHistorial(
        EstadoSolicitud? estadoAnterior,
        EstadoSolicitud estadoNuevo,
        Guid usuarioId,
        string observacion)
    {
        Historial.Add(new HistorialEstado
        {
            Id = Guid.NewGuid(),
            SolicitudId = Id,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = estadoNuevo,
            FechaCambio = FechaActualizacion,
            UsuarioId = usuarioId,
            Observacion = observacion
        });
    }
}
