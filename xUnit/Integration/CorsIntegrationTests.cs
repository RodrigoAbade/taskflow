// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using TaskFlow.Tests.Integration;
using Xunit;

namespace TaskFlow.Tests.Integration;

public class CorsIntegrationTests
{
    private const string AllowedOrigin =
        "http://localhost:4200";

    private const string BlockedOrigin =
        "https://site-nao-autorizado.com";

    private static WebApplicationFactory<Program>
        CreateFactory()
    {
        return new TaskFlowApiFactory()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting(
                    "Cors:AllowedOrigins:0",
                    AllowedOrigin);
            });
    }

    [Fact]
    public async Task Preflight_DevePermitirOrigemAngular()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        using var request = new HttpRequestMessage(
            HttpMethod.Options,
            "/api/projects");

        request.Headers.Add("Origin", AllowedOrigin);
        request.Headers.Add(
            "Access-Control-Request-Method",
            "POST");
        request.Headers.Add(
            "Access-Control-Request-Headers",
            "authorization,content-type");

        using var response =
            await client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        Assert.Equal(
            AllowedOrigin,
            response.Headers.GetValues(
                "Access-Control-Allow-Origin").Single());

        Assert.Contains(
            "POST",
            response.Headers.GetValues(
                "Access-Control-Allow-Methods").Single());

        Assert.True(
            response.Headers.Contains(
                "Access-Control-Allow-Headers"));
    }

    [Fact]
    public async Task Preflight_NaoDevePermitirOrigemDesconhecida()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        using var request = new HttpRequestMessage(
            HttpMethod.Options,
            "/api/projects");

        request.Headers.Add("Origin", BlockedOrigin);
        request.Headers.Add(
            "Access-Control-Request-Method",
            "POST");

        using var response =
            await client.SendAsync(request);

        Assert.False(
            response.Headers.Contains(
                "Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task RequisicaoComOrigemPermitida_DeveReceberCors()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/projects");

        request.Headers.Add("Origin", AllowedOrigin);

        using var response =
            await client.SendAsync(request);

        // A rota exige JWT, então esperamos 401.
        // Mesmo assim, o cabeçalho CORS deve existir.
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);

        Assert.Equal(
            AllowedOrigin,
            response.Headers.GetValues(
                "Access-Control-Allow-Origin").Single());
    }

    [Fact]
    public async Task RequisicaoComOrigemBloqueada_NaoDeveReceberCors()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/projects");

        request.Headers.Add("Origin", BlockedOrigin);

        using var response =
            await client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);

        Assert.False(
            response.Headers.Contains(
                "Access-Control-Allow-Origin"));
    }
}
