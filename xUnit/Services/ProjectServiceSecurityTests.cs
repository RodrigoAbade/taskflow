// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.Tests.Services;

public class ProjectServiceSecurityTests
{
    private static TaskFlowDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TaskFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TaskFlowDbContext(options);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOnlyOwnersProjects()
    {
        using var context = CreateContext();
        var service = new ProjectService(context);

        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var projectA = await service.CreateAsync(
            new CreateProjectDto { Name = "Projeto A" }, userA);

        await service.CreateAsync(
            new CreateProjectDto { Name = "Projeto B" }, userB);

        var projects = (await service.GetAllAsync(userA)).ToList();

        Assert.Single(projects);
        Assert.Equal(projectA.Id, projects[0].Id);
    }

    [Fact]
    public async Task GetById_ShouldNotReturnAnotherUsersProject()
    {
        using var context = CreateContext();
        var service = new ProjectService(context);

        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var project = await service.CreateAsync(
            new CreateProjectDto { Name = "Privado" }, userA);

        var result = await service.GetByIdAsync(project.Id, userB);

        Assert.Null(result);
    }

    [Fact]
    public async Task Update_ShouldNotModifyAnotherUsersProject()
    {
        using var context = CreateContext();
        var service = new ProjectService(context);

        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var project = await service.CreateAsync(
            new CreateProjectDto { Name = "Original" }, userA);

        var result = await service.UpdateAsync(
            project.Id,
            new UpdateProjectDto { Name = "Alterado indevidamente" },
            userB);

        Assert.Null(result);

        var original = await service.GetByIdAsync(
            project.Id, userA);

        Assert.NotNull(original);
        Assert.Equal("Original", original.Name);
    }

    [Fact]
    public async Task Deactivate_ShouldNotDeactivateAnotherUsersProject()
    {
        using var context = CreateContext();
        var service = new ProjectService(context);

        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var project = await service.CreateAsync(
            new CreateProjectDto { Name = "Projeto ativo" }, userA);

        var result = await service.DeactivateAsync(
            project.Id, userB);

        Assert.False(result);

        var original = await service.GetByIdAsync(
            project.Id, userA);

        Assert.NotNull(original);
        Assert.True(original.IsActive);
    }
}