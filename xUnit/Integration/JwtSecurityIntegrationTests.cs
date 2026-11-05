
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Tests.Integration;

public class JwtSecurityIntegrationTests
{
    private const string Key =
        "MDEyMzQ1Njc4OUFCQ0RFRjAxMjM0NTY3ODlBQkNERUY=";

    private static async Task<(Guid Id, string Token)> RegisterAndLogin(
        HttpClient client, string email)
    {
        var register = await client.PostAsJsonAsync(
            "/api/users",
            new
            {
                name = "Integration User",
                email,
                password = "Password123!"
            });

        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email,
                password = "Password123!"
            });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        using var json = JsonDocument.Parse(
            await login.Content.ReadAsStringAsync());

        var root = json.RootElement;

        return (
            root.GetProperty("user").GetProperty("id").GetGuid(),
            root.GetProperty("token").GetString()!
        );
    }

    private static string CreateExpiredToken(Guid userId)
    {
        var key = new SymmetricSecurityKey(
            Convert.FromBase64String(Key));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "TaskFlow.Tests",
            audience: "TaskFlow.Tests.Client",
            claims: new[]
            {
                new Claim("sub", userId.ToString())
            },
            notBefore: DateTime.UtcNow.AddHours(-2),
            expires: DateTime.UtcNow.AddHours(-1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task ExpiredToken_ReturnUnauthorized()
    {
        using var factory = new TaskFlowApiFactory();
        using var client = factory.CreateClient();

        var user = await RegisterAndLogin(
            client, "expired@example.com");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer", CreateExpiredToken(user.Id));

        var response = await client.GetAsync("/api/projects");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task InvalidSignature_ReturnUnauthorized()
    {
        using var factory = new TaskFlowApiFactory();
        using var client = factory.CreateClient();

        var user = await RegisterAndLogin(
            client, "signature@example.com");

        var parts = user.Token.Split('.');
        Assert.Equal(3, parts.Length);

        // Substitui a assinatura por uma assinatura inválida.
        var invalidToken = $"{parts[0]}.{parts[1]}.AAAA";

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer", invalidToken);

        var response = await client.GetAsync("/api/projects");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task DeactivatedUser_CannotAccessProtectedEndpoint()
    {
        using var factory = new TaskFlowApiFactory();
        using var client = factory.CreateClient();

        var user = await RegisterAndLogin(
            client, "deactivated@example.com");

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<TaskFlowDbContext>();

            var entity = await db.Users
                .FirstAsync(x => x.Id == user.Id);

            entity.Deactivate();
            await db.SaveChangesAsync();
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer", user.Token);

        var response = await client.GetAsync("/api/projects");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
}
