// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.DTOs.Users;

namespace TaskFlow.Tests.Integration;

public class ErrorResponseIntegrationTests
{
    [Fact]
    public async Task Login_InvalidCredentials_ReturnProblemDetails()
    {
        using var factory = new TaskFlowApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginDto
            {
                Email = "missing@example.com",
                Password = "InvalidPassword123!"
            });

        await AssertProblemDetails(
            response,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_InvalidData_ReturnProblemDetails()
    {
        using var factory = new TaskFlowApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/users",
            new CreateUserDto
            {
                Name = "",
                Email = "invalid-email",
                Password = ""
            });

        await AssertProblemDetails(
            response,
            HttpStatusCode.BadRequest);
    }

    private static async Task AssertProblemDetails(
        HttpResponseMessage response,
        HttpStatusCode expectedStatusCode)
    {
        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"Resposta HTTP: {(int)response.StatusCode}");

        Console.WriteLine(
            $"Content-Type: {response.Content.Headers.ContentType}");

        Console.WriteLine(
            $"JSON recebido: {responseBody}");

        Assert.Equal(
            expectedStatusCode,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(responseBody);

        var root = document.RootElement;

        Assert.Equal(
            JsonValueKind.Object,
            root.ValueKind);

        Assert.True(
            root.TryGetProperty("status", out var status),
            "A resposta não contém a propriedade 'status'.");

        Assert.Equal(
            (int)expectedStatusCode,
            status.GetInt32());

        Assert.True(
            root.TryGetProperty("title", out var title),
            "A resposta não contém a propriedade 'title'.");

        Assert.False(
            string.IsNullOrWhiteSpace(title.GetString()));

        var hasDetail =
            root.TryGetProperty("detail", out var detail) &&
            detail.ValueKind == JsonValueKind.String &&
            !string.IsNullOrWhiteSpace(detail.GetString());

        var hasErrors =
            root.TryGetProperty("errors", out var errors) &&
            errors.ValueKind == JsonValueKind.Object &&
            errors.EnumerateObject().Any();

        Assert.True(
            hasDetail || hasErrors,
            "A resposta deve conter 'detail' ou erros de validação em 'errors'.");
    }
}
