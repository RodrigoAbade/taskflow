
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.Api.Exceptions;

public sealed class GlobalExceptionHandler
    : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Requisição inválida",
                exception.Message
            ),

            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "Recurso não encontrado",
                exception.Message
            ),

            InvalidOperationException => (
                StatusCodes.Status409Conflict,
                "Conflito de operação",
                exception.Message
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno do servidor",
                "Ocorreu um erro inesperado. Tente novamente mais tarde."
            )
        };

        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Erro inesperado na requisição {Path}",
                httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Erro de aplicação na requisição {Path}",
                httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        return true;
    }
}
