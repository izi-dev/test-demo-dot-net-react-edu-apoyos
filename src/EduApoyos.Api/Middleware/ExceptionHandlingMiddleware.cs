using EduApoyos.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EduApoyos.Api.Middleware;

/// <summary>
/// Traduce excepciones de dominio y aplicación a respuestas RFC 7807.
/// </summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
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
            _ => (StatusCodes.Status500InternalServerError, "Error interno", "Ocurrió un error inesperado.", null)
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
