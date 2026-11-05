// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Application.DTOs.Users;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.Tests.Services;

public class TokenServiceTests
{
    // Chave exclusiva para testes, não utilizada em produção.
    private const string Key =
        "MDEyMzQ1Njc4OUFCQ0RFRjAxMjM0NTY3ODlBQkNERUY=";

    private static IConfiguration Config(bool includeKey = true)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = includeKey ? Key : null,
                ["Jwt:Issuer"] = "TaskFlow.Tests",
                ["Jwt:Audience"] = "TaskFlow.Tests.Client"
            })
            .Build();
    }

    [Fact]
    public void GenerateToken_ContainsUserIdAndEmail()
    {
        var id = Guid.NewGuid();

        var token = new TokenService(Config()).GenerateToken(
            new UserResponseDto
            {
                Id = id,
                Email = "test@example.com"
            });

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(id.ToString(), jwt.Subject);
        Assert.Equal(
            "test@example.com",
            jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
    }

    [Fact]
    public void GenerateToken_HasOneHourExpiry()
    {
        var token = new TokenService(Config()).GenerateToken(
            new UserResponseDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com"
            });

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.InRange(
            jwt.ValidTo - DateTime.UtcNow,
            TimeSpan.FromMinutes(59),
            TimeSpan.FromMinutes(61));
    }

    [Fact]
    public void GenerateToken_HasValidSignature()
    {
        var token = new TokenService(Config()).GenerateToken(
            new UserResponseDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com"
            });

        var handler = new JwtSecurityTokenHandler();

        var principal = handler.ValidateToken(
            token,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "TaskFlow.Tests",

                ValidateAudience = true,
                ValidAudience = "TaskFlow.Tests.Client",

                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                IssuerSigningKey = new SymmetricSecurityKey(
                    Convert.FromBase64String(Key)),

                ClockSkew = TimeSpan.Zero
            },
            out _);

        Assert.NotNull(principal);
    }

    [Fact]
    public void GenerateToken_WithoutKey_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new TokenService(Config(false)).GenerateToken(
                new UserResponseDto
                {
                    Id = Guid.NewGuid(),
                    Email = "test@example.com"
                }));
    }
}
