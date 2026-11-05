// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Tests.Integration;

public class TaskFlowApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName =
        $"TaskFlowTests_{Guid.NewGuid():N}";

    private const string JwtKey =
        "MDEyMzQ1Njc4OUFCQ0RFRjAxMjM0NTY3ODlBQkNERUY=";

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("Jwt:Issuer", "TaskFlow.Tests");
        builder.UseSetting(
            "Jwt:Audience",
            "TaskFlow.Tests.Client");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<
                DbContextOptions<TaskFlowDbContext>>();

            services.RemoveAll<
                IDbContextOptionsConfiguration<TaskFlowDbContext>>();

            services.AddDbContext<TaskFlowDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
