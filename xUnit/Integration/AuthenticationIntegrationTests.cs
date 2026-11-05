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

public class AuthenticationIntegrationTests
{
    [Theory]
    [InlineData("/api/projects")]
    [InlineData("/api/tasks")]
    public async Task ProtectedEndpoints_WithoutToken_ReturnUnauthorized(
        string endpoint)
    {
        using var factory = new TaskFlowApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(endpoint);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnUnauthorized()
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

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task RegisterAndLogin_ReturnValidToken()
    {
        using var factory = new TaskFlowApiFactory();
        using var client = factory.CreateClient();

        var registration = await client.PostAsJsonAsync(
            "/api/users",
            new CreateUserDto
            {
                Name = "Integration Test",
                Email = "integration@example.com",
                Password = "Password123!"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            registration.StatusCode);

        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginDto
            {
                Email = "integration@example.com",
                Password = "Password123!"
            });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        using var body = JsonDocument.Parse(
            await login.Content.ReadAsStringAsync());

        var token = body.RootElement
            .GetProperty("token")
            .GetString();

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task ProtectedEndpoint_InvalidToken_ReturnUnauthorized()
    {
        using var factory = new TaskFlowApiFactory();
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", "invalid-token");

        var response = await client.GetAsync("/api/projects");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
}
