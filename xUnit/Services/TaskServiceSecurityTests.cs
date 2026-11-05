
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Services;
using TaskPriority = TaskFlow.Domain.Enums.TaskPriority;

namespace TaskFlow.Tests.Services;

public class TaskServiceSecurityTests
{
    private static TaskFlowDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TaskFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TaskFlowDbContext(options);
    }

    private static async Task<(Guid userA, Guid userB,
        Project projectA, Project projectB,
        TaskItem taskA, TaskItem taskB)> SeedAsync(
            TaskFlowDbContext context)
    {
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var projectA = new Project("Projeto A", "", userA);
        var projectB = new Project("Projeto B", "", userB);

        var taskA = new TaskItem(
            projectA.Id, "Tarefa A", "",
            TaskPriority.Medium);

        var taskB = new TaskItem(
            projectB.Id, "Tarefa B", "",
            TaskPriority.Medium);

        context.Projects.AddRange(projectA, projectB);
        context.Tasks.AddRange(taskA, taskB);

        await context.SaveChangesAsync();

        return (userA, userB, projectA, projectB, taskA, taskB);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOnlyOwnersTasks()
    {
        using var context = CreateContext();
        var data = await SeedAsync(context);
        var service = new TaskService(context);

        var result = (await service.GetAllAsync(data.userA))
            .ToList();

        Assert.Single(result);
        Assert.Equal(data.taskA.Id, result[0].Id);
    }

    [Fact]
    public async Task GetById_ShouldNotReturnOtherUsersTask()
    {
        using var context = CreateContext();
        var data = await SeedAsync(context);
        var service = new TaskService(context);

        var result = await service.GetByIdAsync(
            data.taskB.Id, data.userA);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByProjectId_ShouldNotReturnOtherUsersTasks()
    {
        using var context = CreateContext();
        var data = await SeedAsync(context);
        var service = new TaskService(context);

        var result = await service.GetByProjectIdAsync(
            data.projectB.Id, data.userA);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Create_ShouldRejectOtherUsersProject()
    {
        using var context = CreateContext();
        var data = await SeedAsync(context);
        var service = new TaskService(context);

        var dto = new CreateTaskDto
        {
            ProjectId = data.projectB.Id,
            Title = "Tarefa indevida",
            Priority = TaskPriority.Medium
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CreateAsync(dto, data.userA));

        Assert.Equal(2, await context.Tasks.CountAsync());
    }

    [Fact]
    public async Task Update_ShouldNotModifyOtherUsersTask()
    {
        using var context = CreateContext();
        var data = await SeedAsync(context);
        var service = new TaskService(context);

        var dto = new UpdateTaskDto
        {
            Title = "Título alterado",
            Priority = TaskPriority.High
        };

        var result = await service.UpdateAsync(
            data.taskB.Id, dto, data.userA);

        Assert.Null(result);

        var original = await context.Tasks
            .AsNoTracking()
            .SingleAsync(t => t.Id == data.taskB.Id);

        Assert.Equal("Tarefa B", original.Title);
        Assert.Equal(TaskPriority.Medium, original.Priority);
    }

    [Fact]
    public async Task Start_ShouldNotStartOtherUsersTask()
    {
        using var context = CreateContext();
        var data = await SeedAsync(context);
        var service = new TaskService(context);

        var result = await service.StartAsync(
            data.taskB.Id, data.userA);

        Assert.Null(result);

        var original = await context.Tasks
            .AsNoTracking()
            .SingleAsync(t => t.Id == data.taskB.Id);

        Assert.Equal(
            TaskFlow.Domain.Enums.TaskStatus.Pending,
            original.Status);
    }

    [Fact]
    public async Task Complete_ShouldNotCompleteOtherUsersTask()
    {
        using var context = CreateContext();
        var data = await SeedAsync(context);
        var service = new TaskService(context);

        data.taskB.Start();
        await context.SaveChangesAsync();

        var result = await service.CompleteAsync(
            data.taskB.Id, data.userA);

        Assert.Null(result);

        var original = await context.Tasks
            .AsNoTracking()
            .SingleAsync(t => t.Id == data.taskB.Id);

        Assert.Equal(
            TaskFlow.Domain.Enums.TaskStatus.InProgress,
            original.Status);
    }

    [Fact]
    public async Task Cancel_ShouldNotCancelOtherUsersTask()
    {
        using var context = CreateContext();
        var data = await SeedAsync(context);
        var service = new TaskService(context);

        var result = await service.CancelAsync(
            data.taskB.Id, data.userA);

        Assert.Null(result);

        var original = await context.Tasks
            .AsNoTracking()
            .SingleAsync(t => t.Id == data.taskB.Id);

        Assert.Equal(
            TaskFlow.Domain.Enums.TaskStatus.Pending,
            original.Status);
    }
}
