using System.Text;
using EduApoyos.Application.Common.Abstractions;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Exceptions;

namespace EduApoyos.Application.Features.Constancias;

/// <summary>
/// Consulta para generar la constancia de una solicitud del estudiante.
/// </summary>
/// <param name="EstudianteId">Identificador del estudiante dueño del portal.</param>
/// <param name="SolicitudId">Identificador de la solicitud a certificar.</param>
/// <param name="UsuarioId">Usuario autenticado; debe ser dueño de <paramref name="EstudianteId"/>.</param>
public sealed record GenerateConstanciaQuery(
    Guid EstudianteId,
    Guid SolicitudId,
    Guid UsuarioId);

/// <summary>
/// Resultado de la constancia en texto plano descargable.
/// </summary>
/// <param name="NombreArchivo">Nombre sugerido del archivo (p. ej. <c>constancia-{id}.txt</c>).</param>
/// <param name="Contenido">Cuerpo de la constancia en texto plano.</param>
/// <param name="ContentType">Tipo MIME; siempre <c>text/plain</c> en esta implementación.</param>
public sealed record ConstanciaResultado(string NombreArchivo, string Contenido, string ContentType);

/// <summary>
/// Genera la constancia validando propiedad del recurso.
/// </summary>
/// <remarks>
/// No tiene validator FluentValidation. Autoriza en dos niveles:
/// (1) el usuario debe ser dueño del <c>EstudianteId</c> del portal;
/// (2) la solicitud debe pertenecer a ese estudiante.
/// El contenido se genera en memoria como texto plano (no PDF).
/// </remarks>
/// <param name="solicitudes">Puerto de lectura de solicitudes.</param>
/// <param name="estudiantes">Puerto de resolución del perfil de estudiante.</param>
public sealed class GenerateConstanciaQueryHandler(
    ISolicitudRepository solicitudes,
    IEstudianteRepository estudiantes) : IQueryHandler<GenerateConstanciaQuery, ConstanciaResultado>
{
    /// <summary>
    /// Genera la constancia de texto plano de una solicitud autorizada.
    /// </summary>
    /// <param name="query">Identificadores de estudiante, solicitud y usuario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>
    /// <see cref="ConstanciaResultado"/> con nombre de archivo, contenido y content-type.
    /// </returns>
    /// <exception cref="AccesoRecursoDenegadoException">
    /// Si el usuario no tiene estudiante, si consulta otro portal, o si la solicitud no le pertenece.
    /// </exception>
    /// <exception cref="RecursoNoEncontradoException">
    /// Si la solicitud no existe.
    /// </exception>
    public async Task<ConstanciaResultado> HandleAsync(GenerateConstanciaQuery query, CancellationToken cancellationToken = default)
    {
        var estudiante = await estudiantes.ObtenerPorUsuarioIdAsync(query.UsuarioId, cancellationToken)
            ?? throw new AccesoRecursoDenegadoException("El usuario no tiene estudiante asociado.");

        if (estudiante.Id != query.EstudianteId)
        {
            throw new AccesoRecursoDenegadoException("Solo puedes descargar constancias de tu portal.");
        }

        var solicitud = await solicitudes.ObtenerPorIdConDetalleAsync(query.SolicitudId, cancellationToken)
            ?? throw new RecursoNoEncontradoException("solicitud", query.SolicitudId);

        if (!solicitud.PerteneceA(estudiante.Id))
        {
            throw new AccesoRecursoDenegadoException("Solo puedes descargar constancias de tus solicitudes.");
        }

        var contenido = new StringBuilder()
            .AppendLine("Constancia de solicitud de apoyo económico")
            .AppendLine($"Solicitud: {solicitud.Id}")
            .AppendLine($"Estudiante: {solicitud.Estudiante?.Usuario?.NombreCompleto}")
            .AppendLine($"Tipo de apoyo: {solicitud.TipoApoyo}")
            .AppendLine($"Monto solicitado: {solicitud.MontoSolicitado:C}")
            .AppendLine($"Estado actual: {solicitud.Estado}")
            .AppendLine($"Fecha de solicitud: {solicitud.FechaSolicitud:yyyy-MM-dd}")
            .ToString();

        return new ConstanciaResultado(
            NombreArchivo: $"constancia-{solicitud.Id}.txt",
            Contenido: contenido,
            ContentType: "text/plain");
    }
}
