// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.Api.Helpers;

public static class ApiErrors
{
    public static ObjectResult Create(
        int statusCode,
        string detail)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode switch
            {
                400 => "Requisição inválida",
                401 => "Não autenticado",
                403 => "Acesso negado",
                404 => "Recurso não encontrado",
                409 => "Conflito de operação",
                _ => "Erro"
            },
            Detail = detail
        };

        return new ObjectResult(problem)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }

    public static ObjectResult BadRequest(string detail) =>
        Create(400, detail);

    public static ObjectResult Unauthorized(string detail) =>
        Create(401, detail);

    public static ObjectResult NotFound(string detail) =>
        Create(404, detail);

    public static ObjectResult Conflict(string detail) =>
        Create(409, detail);
}
