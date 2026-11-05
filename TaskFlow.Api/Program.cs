
using System;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Api.Exceptions;
using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Services;
using TaskFlow.Api.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// CONTROLLERS E OPENAPI
// ========================================

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<
        JwtSecurityDocumentTransformer>();
});
// ========================================
// CORS - INTEGRAÇÃO COM ANGULAR
// ========================================

const string corsPolicy = "TaskFlowFrontend";

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

// ========================================
// TRATAMENTO GLOBAL DE ERROS
// ========================================

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<
    GlobalExceptionHandler>();

// ========================================
// BANCO DE DADOS
// ========================================

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<TaskFlowDbContext>(options =>
    options.UseSqlServer(connectionString));

// ========================================
// SERVIÇOS DA APLICAÇÃO
// ========================================

builder.Services.AddScoped<
    IProjectService, ProjectService>();

builder.Services.AddScoped<
    ITaskService, TaskService>();

builder.Services.AddScoped<
    IUserService, UserService>();

builder.Services.AddScoped<
    IPasswordService, PasswordService>();

builder.Services.AddScoped<
    IAuthService, AuthService>();

builder.Services.AddScoped<
    ITokenService, TokenService>();

// ========================================
// AUTENTICAÇÃO JWT
// ========================================

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "A chave JWT não foi configurada.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "O emissor JWT não foi configurado.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "A audiência JWT não foi configurada.");

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userIdClaim = context.Principal?
                    .FindFirst("sub")?.Value;

                if (!Guid.TryParse(
                    userIdClaim, out var userId))
                {
                    context.Fail(
                        "Token sem identificação válida.");

                    return;
                }

                var dbContext = context.HttpContext
                    .RequestServices
                    .GetRequiredService<
                        TaskFlowDbContext>();

                var isActive = await dbContext.Users
                    .AsNoTracking()
                    .AnyAsync(
                        user =>
                            user.Id == userId &&
                            user.IsActive,
                        context.HttpContext.RequestAborted);

                if (!isActive)
                {
                    context.Fail(
                        "Usuário inexistente ou desativado.");
                }
            }
        };

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Convert.FromBase64String(jwtKey))
            };
    });

builder.Services.AddAuthorization();

// ========================================
// RATE LIMITING - LOGIN
// ========================================

builder.Services.AddRateLimiter(options =>
{
    // Máximo de 5 tentativas por minuto, por IP
    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection
                .RemoteIpAddress?
                .MapToIPv6()
                .ToString() ?? "unknown",

            factory: _ =>
                new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,

                    Window =
                        TimeSpan.FromMinutes(1),

                    QueueLimit = 0,

                    AutoReplenishment = true
                }));

    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (
        context, cancellationToken) =>
    {
        var response = context.HttpContext.Response;

        response.StatusCode =
            StatusCodes.Status429TooManyRequests;

        var problem = new ProblemDetails
        {
            Status = 429,

            Title = "Muitas requisições",

            Detail =
                "Limite de tentativas atingido. " +
                "Aguarde um minuto antes de tentar novamente."
        };

        await response.WriteAsJsonAsync(
            problem,
            options:
                new System.Text.Json.JsonSerializerOptions(
                    System.Text.Json.JsonSerializerDefaults.Web),
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);
    };
});

// ========================================
// CONSTRUÇÃO DA APLICAÇÃO
// ========================================

var app = builder.Build();

// ========================================
// MIDDLEWARES
// ========================================

// Tratamento global de exceções
app.UseExceptionHandler();

// OpenAPI e Swagger UI
// Disponíveis apenas em desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "TaskFlow API v1");

        options.RoutePrefix = "swagger";

        options.DocumentTitle =
            "TaskFlow API - Swagger UI";
    });
}

// Redirecionamento HTTPS
app.UseHttpsRedirection();

// Roteamento
app.UseRouting();

// CORS - deve ficar depois do roteamento
// e antes da autenticação e autorização
app.UseCors(corsPolicy);

// Proteção contra excesso de requisições
app.UseRateLimiter();

// Autenticação JWT
app.UseAuthentication();

// Autorização
app.UseAuthorization();

// Controllers
app.MapControllers();

// ========================================
// INICIALIZAÇÃO
// ========================================

app.Run();

// Necessário para testes de integração
public partial class Program { }
