using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Exceptions;
using EduApoyos.Domain.ValueObjects;

namespace EduApoyos.Domain.Entities;

/// <summary>
/// Agregado raíz que representa una solicitud de apoyo económico y su ciclo de vida.
/// </summary>
public class SolicitudApoyo
{
    /// <summary>
    /// Constructor privado requerido por el ORM para materializar la entidad.
    /// </summary>
    private SolicitudApoyo()
    {
    }

    /// <summary>
    /// Identificador único de la solicitud.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Identificador del estudiante que presenta la solicitud.
    /// </summary>
    public Guid EstudianteId { get; private set; }

    /// <summary>
    /// Navegación al estudiante asociado a la solicitud.
    /// </summary>
    public Estudiante? Estudiante { get; private set; }

    /// <summary>
    /// Tipo de apoyo económico solicitado.
    /// </summary>
    public TipoApoyo TipoApoyo { get; private set; }

    /// <summary>
    /// Monto económico solicitado, expresado como decimal.
    /// </summary>
    public decimal MontoSolicitado { get; private set; }

    /// <summary>
    /// Descripción o justificación de la solicitud.
    /// </summary>
    public string Descripcion { get; private set; } = string.Empty;

    /// <summary>
    /// Estado actual de la solicitud en el flujo institucional.
    /// </summary>
    public EstadoSolicitud Estado { get; private set; } = EstadoSolicitud.Pendiente;

    /// <summary>
    /// Fecha y hora UTC en que se registró la solicitud.
    /// </summary>
    public DateTime FechaSolicitud { get; private set; }

    /// <summary>
    /// Fecha y hora UTC de la última actualización de la solicitud.
    /// </summary>
    public DateTime FechaActualizacion { get; private set; }

    /// <summary>
    /// Identificador del asesor asignado; nulo si aún no hay asesor.
    /// </summary>
    public Guid? AsesorId { get; private set; }

    /// <summary>
    /// Navegación al asesor asignado a la solicitud.
    /// </summary>
    public Usuario? Asesor { get; private set; }

    /// <summary>
    /// Colección de registros de auditoría de cambios de estado.
    /// </summary>
    public ICollection<HistorialEstado> Historial { get; private set; } = new List<HistorialEstado>();

    /// <summary>
    /// Crea una nueva solicitud en estado <see cref="EstadoSolicitud.Pendiente"/>.
    /// </summary>
    /// <param name="estudianteId">Identificador del estudiante solicitante.</param>
    /// <param name="estudiante">Entidad del estudiante asociada a la solicitud.</param>
    /// <param name="tipoApoyo">Tipo de apoyo económico solicitado.</param>
    /// <param name="monto">Monto validado mediante el value object <see cref="MontoSolicitado"/>.</param>
    /// <param name="descripcion">Descripción validada mediante el value object <see cref="DescripcionSolicitud"/>.</param>
    /// <param name="usuarioCreadorId">Identificador del usuario que crea la solicitud (para el historial).</param>
    /// <returns>Una instancia de <see cref="SolicitudApoyo"/> en estado pendiente con el primer registro de historial.</returns>
    /// <remarks>
    /// Reglas aplicadas al crear la solicitud:
    /// <list type="bullet">
    /// <item><description>Se genera un nuevo <see cref="Id"/> con <see cref="Guid.NewGuid"/>.</description></item>
    /// <item><description>El estado inicial es siempre <see cref="EstadoSolicitud.Pendiente"/>.</description></item>
    /// <item><description><see cref="FechaSolicitud"/> y <see cref="FechaActualizacion"/> se establecen en UTC al momento de la creación.</description></item>
    /// <item><description>El monto y la descripción se toman de los value objects ya validados (<c>monto.Valor</c> y <c>descripcion.Valor</c>).</description></item>
    /// <item><description>Se registra automáticamente un historial con observación "Solicitud creada." y <c>estadoAnterior</c> nulo.</description></item>
    /// </list>
    /// </remarks>
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
    /// <param name="nuevoEstado">Estado destino al que se desea transicionar.</param>
    /// <param name="usuarioId">Identificador del usuario que ejecuta el cambio.</param>
    /// <param name="observacion">Observación opcional asociada al cambio; si es nula se registra como cadena vacía.</param>
    /// <param name="asesorId">Identificador opcional del asesor a asignar; si es nulo se conserva el asesor actual.</param>
    /// <exception cref="TransicionEstadoInvalidaException">
    /// Se lanza cuando la transición desde el estado actual hacia <paramref name="nuevoEstado"/> no está permitida.
    /// </exception>
    /// <remarks>
    /// Transiciones permitidas (ver también <see cref="PuedeCambiarA"/>):
    /// <list type="bullet">
    /// <item><description><see cref="EstadoSolicitud.Pendiente"/> → <see cref="EstadoSolicitud.EnRevision"/>.</description></item>
    /// <item><description><see cref="EstadoSolicitud.EnRevision"/> → <see cref="EstadoSolicitud.Aprobada"/> o <see cref="EstadoSolicitud.Rechazada"/>.</description></item>
    /// <item><description>Cualquier otra transición (incluyendo desde estados terminales) se rechaza.</description></item>
    /// </list>
    /// Efectos colaterales: actualiza <see cref="FechaActualizacion"/> a UTC, asigna o conserva <see cref="AsesorId"/>
    /// y agrega un registro en <see cref="Historial"/>.
    /// </remarks>
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
    /// Devuelve el último registro de historial (el más reciente por fecha).
    /// </summary>
    /// <returns>
    /// El <see cref="HistorialEstado"/> con la <see cref="HistorialEstado.FechaCambio"/> más reciente,
    /// o <c>null</c> si la colección de historial está vacía.
    /// </returns>
    public HistorialEstado? UltimoHistorial() =>
        Historial.OrderByDescending(x => x.FechaCambio).FirstOrDefault();

    /// <summary>
    /// Evalúa si la transición al estado destino está permitida.
    /// </summary>
    /// <param name="nuevoEstado">Estado destino a evaluar.</param>
    /// <returns>
    /// <c>true</c> si la transición desde el estado actual hacia <paramref name="nuevoEstado"/> es válida;
    /// de lo contrario, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Reglas de transición:
    /// <list type="bullet">
    /// <item><description>Desde <see cref="EstadoSolicitud.Pendiente"/> solo se permite <see cref="EstadoSolicitud.EnRevision"/>.</description></item>
    /// <item><description>Desde <see cref="EstadoSolicitud.EnRevision"/> se permite <see cref="EstadoSolicitud.Aprobada"/> o <see cref="EstadoSolicitud.Rechazada"/>.</description></item>
    /// <item><description>Desde <see cref="EstadoSolicitud.Aprobada"/> o <see cref="EstadoSolicitud.Rechazada"/> no hay transiciones permitidas.</description></item>
    /// </list>
    /// </remarks>
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
    /// <param name="estudianteId">Identificador del estudiante a comparar.</param>
    /// <returns><c>true</c> si <see cref="EstudianteId"/> coincide con <paramref name="estudianteId"/>; de lo contrario, <c>false</c>.</returns>
    public bool PerteneceA(Guid estudianteId) => EstudianteId == estudianteId;

    /// <summary>
    /// Agrega un registro de auditoría al historial de la solicitud.
    /// </summary>
    /// <param name="estadoAnterior">Estado previo; nulo cuando la solicitud se crea por primera vez.</param>
    /// <param name="estadoNuevo">Estado resultante del cambio.</param>
    /// <param name="usuarioId">Identificador del usuario responsable del cambio.</param>
    /// <param name="observacion">Texto de observación asociado al cambio.</param>
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
