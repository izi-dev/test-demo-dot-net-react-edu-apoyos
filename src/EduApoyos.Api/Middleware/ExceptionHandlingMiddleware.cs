using EduApoyos.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EduApoyos.Api.Middleware;

/// <summary>
/// Middleware que captura excepciones no manejadas y las traduce a respuestas
/// <see cref="ProblemDetails"/> según RFC 7807.
/// </summary>
/// <remarks>
/// Mapeo de excepciones a códigos HTTP:
/// <list type="bullet">
/// <item>
/// <description>
/// <see cref="ValidationException"/> (FluentValidation) → 400 Bad Request,
/// con extensión <c>errors</c> agrupada por nombre de propiedad.
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="RecursoNoEncontradoException"/> → 404 Not Found.
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="AccesoRecursoDenegadoException"/> → 403 Forbidden.
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="TransicionEstadoInvalidaException"/> → 400 Bad Request
/// (transición de estado de solicitud no permitida).
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="DomainException"/> (otras reglas de negocio) → 400 Bad Request.
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="UnauthorizedAccessException"/> → 401 Unauthorized.
/// </description>
/// </item>
/// <item>
/// <description>
/// Cualquier otra excepción → 500 Internal Server Error
/// (detalle del mensaje interno o del mensaje principal).
/// </description>
/// </item>
/// </list>
/// </remarks>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    /// <summary>
    /// Invoca el siguiente middleware y, si ocurre una excepción, escribe el problema HTTP.
    /// </summary>
    /// <param name="context">Contexto HTTP de la solicitud actual.</param>
    /// <returns>Una tarea que representa el procesamiento asíncrono.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Error controlado procesando la solicitud.");
            await WriteProblemAsync(context, exception);
        }
    }

    /// <summary>
    /// Clasifica la excepción, asigna el código de estado y serializa <see cref="ProblemDetails"/>.
    /// </summary>
    /// <param name="context">Contexto HTTP donde se escribe la respuesta.</param>
    /// <param name="exception">Excepción capturada en el pipeline.</param>
    /// <returns>Una tarea que completa cuando la respuesta se ha escrito.</returns>
    private static async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        var (status, title, detail, errors) = exception switch
        {
            ValidationException validation => (
                StatusCodes.Status400BadRequest,
                "Solicitud inválida",
                "Uno o más campos no cumplen las reglas de validación.",
                validation.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(x => x.Key, x => x.Select(e => e.ErrorMessage).ToArray())),
            RecursoNoEncontradoException notFound => (StatusCodes.Status404NotFound, "Recurso no encontrado", notFound.Message, null),
            AccesoRecursoDenegadoException => (StatusCodes.Status403Forbidden, "Acceso denegado", exception.Message, null),
            TransicionEstadoInvalidaException => (StatusCodes.Status400BadRequest, "Transición inválida", exception.Message, null),
            DomainException => (StatusCodes.Status400BadRequest, "Regla de negocio", exception.Message, null),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "No autorizado", exception.Message, null),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno",
                exception.InnerException?.Message ?? exception.Message,
                null)
        };

        context.Response.StatusCode = status;
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        await context.Response.WriteAsJsonAsync(problem);
    }
}
