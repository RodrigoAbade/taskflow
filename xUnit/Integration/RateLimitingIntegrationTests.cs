
using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.DTOs.Auth;

namespace TaskFlow.Tests.Integration;

public class RateLimitingIntegrationTests
{
    [Fact]
    public async Task Login_AfterFiveAttempts_ReturnTooManyRequests()
    {
        using var factory = new TaskFlowApiFactory();
        using var client = factory.CreateClient();

        var login = new LoginDto
        {
            Email = "invalid@example.com",
            Password = "InvalidPassword123!"
        };

        // As cinco primeiras tentativas devem ser permitidas
        for (int attempt = 1; attempt <= 5; attempt++)
        {
            using var response = await client.PostAsJsonAsync(
                "/api/auth/login",
                login);

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        // A sexta tentativa deve ser bloqueada
        using var blockedResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            login);

        Assert.Equal(
            HttpStatusCode.TooManyRequests,
            blockedResponse.StatusCode);

        Assert.Equal(
            "application/problem+json",
            blockedResponse.Content.Headers.ContentType?.MediaType);

        var responseBody =
            await blockedResponse.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(responseBody);

        var root = document.RootElement;

        Assert.Equal(
            429,
            root.GetProperty("status").GetInt32());

        Assert.False(string.IsNullOrWhiteSpace(
            root.GetProperty("title").GetString()));

        Assert.False(string.IsNullOrWhiteSpace(
            root.GetProperty("detail").GetString()));
    }

    [Fact]
    public async Task Login_ShouldHaveRateLimitingPolicy()
    {
        using var factory = new TaskFlowApiFactory();
        using var client = factory.CreateClient();

        // Recupera os endpoints registrados
        var endpointSources = factory.Services
            .GetServices<EndpointDataSource>();

        var loginEndpoints = endpointSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Where(endpoint =>
                string.Equals(
                    endpoint.RoutePattern.RawText?
                        .TrimStart('/'),
                    "api/auth/login",
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        Console.WriteLine(
            $"Endpoints encontrados: {loginEndpoints.Count}");

        Assert.NotEmpty(loginEndpoints);

        foreach (var endpoint in loginEndpoints)
        {
            Console.WriteLine("=== ENDPOINT ===");
            Console.WriteLine(endpoint.DisplayName);

            Console.WriteLine("=== METADADOS ===");

            foreach (var metadata in endpoint.Metadata)
            {
                Console.WriteLine(
                    metadata.GetType().FullName);
            }

            var policy = endpoint.Metadata
                .GetMetadata<EnableRateLimitingAttribute>();

            Console.WriteLine(
                $"Política encontrada: {policy?.PolicyName ?? "NENHUMA"}");
        }

        // Confirma que pelo menos um endpoint
        // possui a política de Rate Limiting
        Assert.Contains(
            loginEndpoints,
            endpoint =>
            {
                var policy = endpoint.Metadata
                    .GetMetadata<EnableRateLimitingAttribute>();

                return policy?.PolicyName == "login";
            });
    }
}
