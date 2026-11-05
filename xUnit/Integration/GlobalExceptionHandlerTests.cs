
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using TaskFlow.Api.Exceptions;

namespace TaskFlow.Tests.Integration;

public class GlobalExceptionHandlerTests
{
    private static async Task<(int Status, JsonElement Body)>
        ExecuteHandler(Exception exception)
    {
        var context = new DefaultHttpContext();

        context.Request.Path = "/api/test";
        context.Response.Body = new MemoryStream();

        var handler = new GlobalExceptionHandler(
            NullLogger<GlobalExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(
            context,
            exception,
            CancellationToken.None);

        Assert.True(handled);

        context.Response.Body.Position = 0;

        using var document = await JsonDocument.ParseAsync(
            context.Response.Body);

        return (
            context.Response.StatusCode,
            document.RootElement.Clone()
        );
    }

    [Fact]
    public async Task ArgumentException_Returns400()
    {
        var result = await ExecuteHandler(
            new ArgumentException("Dados inválidos."));

        Assert.Equal(400, result.Status);

        Assert.Equal(
            "Requisição inválida",
            result.Body.GetProperty("title").GetString());
    }

    [Fact]
    public async Task KeyNotFoundException_Returns404()
    {
        var result = await ExecuteHandler(
            new KeyNotFoundException("Não encontrado."));

        Assert.Equal(404, result.Status);
    }

    [Fact]
    public async Task InvalidOperationException_Returns409()
    {
        var result = await ExecuteHandler(
            new InvalidOperationException("Operação inválida."));

        Assert.Equal(409, result.Status);
    }

    [Fact]
    public async Task UnexpectedException_Returns500WithoutInternalDetails()
    {
        var result = await ExecuteHandler(
            new Exception("Senha interna secreta"));

        Assert.Equal(500, result.Status);

        var detail = result.Body
            .GetProperty("detail")
            .GetString();

        Assert.DoesNotContain(
            "Senha interna secreta",
            detail);
    }

    [Fact]
    public async Task ErrorResponse_ContainsTraceId()
    {
        var result = await ExecuteHandler(
            new Exception("Erro de teste"));

        Assert.True(
            result.Body.TryGetProperty(
                "traceId", out var traceId));

        Assert.False(
            string.IsNullOrWhiteSpace(
                traceId.GetString()));
    }
}
