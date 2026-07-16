using System.Text;
using EduApoyos.Application.Common.Abstractions;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Exceptions;

namespace EduApoyos.Application.Features.Constancias;

/// <summary>
/// Consulta para generar la constancia de una solicitud del estudiante.
/// </summary>
public sealed record GenerateConstanciaQuery(
    Guid EstudianteId,
    Guid SolicitudId,
    Guid UsuarioId);

/// <summary>
/// Resultado de la constancia en texto plano descargable.
/// </summary>
public sealed record ConstanciaResultado(string NombreArchivo, string Contenido, string ContentType);

/// <summary>
/// Genera la constancia validando propiedad del recurso.
/// </summary>
public sealed class GenerateConstanciaQueryHandler(
    ISolicitudRepository solicitudes,
    IEstudianteRepository estudiantes) : IQueryHandler<GenerateConstanciaQuery, ConstanciaResultado>
{
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
