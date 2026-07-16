using EduApoyos.Application.Common;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.Ports;

/// <summary>
/// Puerto de persistencia del agregado <see cref="SolicitudApoyo"/>.
/// </summary>
/// <remarks>
/// Abstrae la lectura y escritura de solicitudes de apoyo sin acoplar los casos de uso
/// a EF Core ni a detalles de tracking. Las consultas de detalle deben incluir estudiante,
/// usuario del estudiante e historial de estados para poder proyectar DTOs completos.
/// </remarks>
public interface ISolicitudRepository
{
    /// <summary>
    /// Obtiene una solicitud por su identificador incluyendo el grafo de detalle necesario para lectura.
    /// </summary>
    /// <param name="id">Identificador único de la solicitud.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>
    /// La solicitud con estudiante, usuario e historial cargados; o <c>null</c> si no existe.
    /// </returns>
    /// <remarks>
    /// Comportamiento esperado de la implementación:
    /// <list type="bullet">
    /// <item>Incluye <c>Estudiante</c> → <c>Usuario</c> e <c>Historial</c> (eager loading).</item>
    /// <item>No lanza excepción si el identificador no existe; devuelve <c>null</c>.</item>
    /// <item>Se usa tanto para consultas de detalle como antes/después de cambios de estado.</item>
    /// </list>
    /// </remarks>
    Task<SolicitudApoyo?> ObtenerPorIdConDetalleAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista solicitudes de forma paginada aplicando los criterios del filtro del asesor.
    /// </summary>
    /// <param name="filtro">
    /// Criterios opcionales de estado, tipo de apoyo, rango de fechas y parámetros de paginación.
    /// </param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>
    /// Un <see cref="PagedResult{T}"/> con la página de solicitudes (con detalle), el total de coincidencias
    /// y los metadatos de paginación.
    /// </returns>
    /// <remarks>
    /// Comportamiento esperado de la implementación:
    /// <list type="bullet">
    /// <item>Si <see cref="FiltroSolicitudes.Estado"/> tiene valor, filtra por ese estado.</item>
    /// <item>Si <see cref="FiltroSolicitudes.TipoApoyo"/> tiene valor, filtra por ese tipo.</item>
    /// <item>Si <see cref="FiltroSolicitudes.Desde"/> tiene valor, exige <c>FechaSolicitud &gt;= Desde</c>.</item>
    /// <item>Si <see cref="FiltroSolicitudes.Hasta"/> tiene valor, exige <c>FechaSolicitud &lt;= Hasta</c>.</item>
    /// <item>Ordena por <c>FechaSolicitud</c> descendente (más recientes primero).</item>
    /// <item>Aplica <c>Skip</c>/<c>Take</c> según <see cref="FiltroSolicitudes.Pagina"/> y <see cref="FiltroSolicitudes.TamanoPagina"/>.</item>
    /// <item>Incluye el mismo grafo de detalle que <see cref="ObtenerPorIdConDetalleAsync"/>.</item>
    /// <item>Devuelve colección vacía (no <c>null</c>) cuando no hay coincidencias.</item>
    /// </list>
    /// </remarks>
    Task<PagedResult<SolicitudApoyo>> ListarAsync(FiltroSolicitudes filtro, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista todas las solicitudes asociadas a un estudiante concreto.
    /// </summary>
    /// <param name="estudianteId">Identificador del estudiante propietario de las solicitudes.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>
    /// Colección de solo lectura de solicitudes del estudiante, con detalle cargado.
    /// Puede estar vacía si el estudiante no tiene solicitudes.
    /// </returns>
    /// <remarks>
    /// Comportamiento esperado de la implementación:
    /// <list type="bullet">
    /// <item>Filtra estrictamente por <c>EstudianteId == estudianteId</c>.</item>
    /// <item>Ordena por <c>FechaSolicitud</c> descendente.</item>
    /// <item>No pagina; devuelve el conjunto completo del estudiante.</item>
    /// <item>Incluye estudiante, usuario e historial para proyección a DTO.</item>
    /// <item>No valida autorización: esa responsabilidad es del caso de uso que invoca el puerto.</item>
    /// </list>
    /// </remarks>
    Task<IReadOnlyCollection<SolicitudApoyo>> ListarPorEstudianteAsync(Guid estudianteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca una nueva solicitud para inserción en el almacén de persistencia.
    /// </summary>
    /// <param name="solicitud">Agregado de solicitud recién creado en el dominio.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona de encolado.</returns>
    /// <remarks>
    /// Comportamiento esperado de la implementación:
    /// <list type="bullet">
    /// <item>Solo añade la entidad al contexto/unidad de trabajo; no confirma la transacción.</item>
    /// <item>La persistencia efectiva ocurre al invocar <see cref="IUnitOfWork.SaveChangesAsync"/>.</item>
    /// <item>Debe incluir el historial inicial asociado al agregado si forma parte del grafo.</item>
    /// <item>No valida reglas de negocio: se asume que el agregado ya fue construido vía fábrica de dominio.</item>
    /// </list>
    /// </remarks>
    Task AgregarAsync(SolicitudApoyo solicitud, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persiste un cambio de estado sin depender del grafo de navegación de EF
    /// (evita conflictos de tracking entre Asesor e Historial).
    /// </summary>
    /// <param name="solicitud">
    /// Agregado ya mutado en memoria (nuevo estado, fecha de actualización y asesor asignados).
    /// </param>
    /// <param name="nuevoHistorial">
    /// Entrada de historial generada por el dominio al cambiar el estado; se insertará de forma aislada.
    /// </param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Una tarea que representa la confirmación del cambio de estado y del historial.</returns>
    /// <remarks>
    /// Comportamiento esperado de la implementación:
    /// <list type="bullet">
    /// <item>
    /// Actualiza de forma escalar (sin grafo) las propiedades <c>Estado</c>, <c>FechaActualizacion</c>
    /// y <c>AsesorId</c> de la fila correspondiente a <c>solicitud.Id</c>.
    /// </item>
    /// <item>
    /// Si ninguna fila es actualizada, debe señalar que la solicitud no existe
    /// (p. ej. <c>RecursoNoEncontradoException</c>).
    /// </item>
    /// <item>
    /// Limpia el change tracker antes de insertar el historial para evitar conflictos
    /// de tracking entre entidades Asesor/Usuario/Historial ya cargadas.
    /// </item>
    /// <item>
    /// Inserta únicamente <paramref name="nuevoHistorial"/> y confirma los cambios
    /// (esta operación sí persiste; no depende de un <see cref="IUnitOfWork"/> externo).
    /// </item>
    /// <item>
    /// Tras completarse, el llamador debe recargar la solicitud con
    /// <see cref="ObtenerPorIdConDetalleAsync"/> si necesita devolver un DTO completo.
    /// </item>
    /// </list>
    /// </remarks>
    Task PersistirCambioEstadoAsync(SolicitudApoyo solicitud, HistorialEstado nuevoHistorial, CancellationToken cancellationToken = default);
}

/// <summary>
/// Criterios de búsqueda para el listado de solicitudes del asesor.
/// </summary>
/// <param name="Estado">Estado opcional por el que filtrar; <c>null</c> incluye todos.</param>
/// <param name="TipoApoyo">Tipo de apoyo opcional; <c>null</c> incluye todos.</param>
/// <param name="Desde">Fecha mínima inclusiva de <c>FechaSolicitud</c>; <c>null</c> sin límite inferior.</param>
/// <param name="Hasta">Fecha máxima inclusiva de <c>FechaSolicitud</c>; <c>null</c> sin límite superior.</param>
/// <param name="Pagina">Número de página (base 1). Valor por defecto: 1.</param>
/// <param name="TamanoPagina">Cantidad de elementos por página. Valor por defecto: 10.</param>
/// <remarks>
/// Este record es un DTO de filtro de aplicación; la normalización de página/tamaño
/// (valores fuera de rango) puede realizarse en el handler antes de construir la instancia.
/// </remarks>
public sealed record FiltroSolicitudes(
    EstadoSolicitud? Estado,
    TipoApoyo? TipoApoyo,
    DateTime? Desde,
    DateTime? Hasta,
    int Pagina = 1,
    int TamanoPagina = 10);
