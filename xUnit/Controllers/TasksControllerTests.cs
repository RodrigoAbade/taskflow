// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Controllers;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Tests.Controllers;

public class TasksControllerTests
{
    private static TasksController CreateController(
        ITaskService service,
        Guid? userId = null)
    {
        var claims = new List<Claim>();

        if (userId.HasValue)
        {
            claims.Add(new Claim(
                "sub", userId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(
            claims, "TestAuthentication");

        var principal = new ClaimsPrincipal(identity);

        return new TasksController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            }
        };
    }

    private static void AssertProblemDetails(
        IActionResult result,
        int expectedStatusCode)
    {
        var objectResult = Assert.IsType<ObjectResult>(result);

        Assert.Equal(expectedStatusCode, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(
            objectResult.Value);

        Assert.Equal(expectedStatusCode, problem.Status);
        Assert.False(string.IsNullOrWhiteSpace(problem.Detail));

        Assert.Contains(
            "application/problem+json",
            objectResult.ContentTypes);
    }

    [Fact]
    public async Task GetAll_ShouldReturnUnauthorized_WhenUserMissing()
    {
        var service = new FakeTaskService();
        var controller = CreateController(service);

        var result = await controller.GetAll();

        Assert.IsType<UnauthorizedResult>(result);
        Assert.False(service.WasCalled);
    }

    [Fact]
    public async Task GetAll_ShouldUseAuthenticatedUserId()
    {
        var userId = Guid.NewGuid();
        var service = new FakeTaskService();
        var controller = CreateController(service, userId);

        var result = await controller.GetAll();

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(userId, service.ReceivedOwnerUserId);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenTaskMissing()
    {
        var service = new FakeTaskService();
        var controller = CreateController(
            service, Guid.NewGuid());

        var result = await controller.GetById(Guid.NewGuid());

        AssertProblemDetails(result, 404);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenUserMissing()
    {
        var service = new FakeTaskService();
        var controller = CreateController(service);

        var result = await controller.Create(new CreateTaskDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Tarefa teste",
            Priority = TaskPriority.Medium
        });

        Assert.IsType<UnauthorizedResult>(result);
        Assert.False(service.WasCalled);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenTitleMissing()
    {
        var service = new FakeTaskService();
        var controller = CreateController(
            service, Guid.NewGuid());

        var result = await controller.Create(new CreateTaskDto
        {
            ProjectId = Guid.NewGuid(),
            Title = ""
        });

        AssertProblemDetails(result, 400);
        Assert.False(service.WasCalled);
    }

    [Fact]
    public async Task Create_ShouldUseAuthenticatedUserId()
    {
        var userId = Guid.NewGuid();
        var service = new FakeTaskService();
        var controller = CreateController(service, userId);

        var result = await controller.Create(new CreateTaskDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Nova tarefa",
            Priority = TaskPriority.Medium
        });

        Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(userId, service.ReceivedOwnerUserId);
    }

    [Fact]
    public async Task Create_ShouldReturnNotFound_WhenProjectNotOwned()
    {
        var service = new FakeTaskService
        {
            ThrowProjectNotFound = true
        };

        var controller = CreateController(
            service, Guid.NewGuid());

        var result = await controller.Create(new CreateTaskDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Tarefa indevida",
            Priority = TaskPriority.Medium
        });

        AssertProblemDetails(result, 404);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenTaskMissing()
    {
        var service = new FakeTaskService();
        var controller = CreateController(
            service, Guid.NewGuid());

        var result = await controller.Update(
            Guid.NewGuid(),
            new UpdateTaskDto
            {
                Title = "Tarefa atualizada",
                Priority = TaskPriority.High
            });

        AssertProblemDetails(result, 404);
    }

    [Fact]
    public async Task Start_ShouldReturnNotFound_WhenTaskMissing()
    {
        var service = new FakeTaskService();
        var controller = CreateController(
            service, Guid.NewGuid());

        var result = await controller.Start(Guid.NewGuid());

        AssertProblemDetails(result, 404);
    }

    [Fact]
    public async Task Complete_ShouldReturnNotFound_WhenTaskMissing()
    {
        var service = new FakeTaskService();
        var controller = CreateController(
            service, Guid.NewGuid());

        var result = await controller.Complete(Guid.NewGuid());

        AssertProblemDetails(result, 404);
    }

    [Fact]
    public async Task Cancel_ShouldReturnNotFound_WhenTaskMissing()
    {
        var service = new FakeTaskService();
        var controller = CreateController(
            service, Guid.NewGuid());

        var result = await controller.Cancel(Guid.NewGuid());

        AssertProblemDetails(result, 404);
    }

    private class FakeTaskService : ITaskService
    {
        public Guid ReceivedOwnerUserId { get; private set; }
        public bool WasCalled { get; private set; }
        public bool ThrowProjectNotFound { get; set; }

        private void Track(Guid ownerUserId)
        {
            WasCalled = true;
            ReceivedOwnerUserId = ownerUserId;
        }

        public Task<TaskResponseDto> CreateAsync(
            CreateTaskDto dto, Guid ownerUserId)
        {
            Track(ownerUserId);

            if (ThrowProjectNotFound)
                throw new KeyNotFoundException();

            return Task.FromResult(new TaskResponseDto
            {
                Id = Guid.NewGuid(),
                ProjectId = dto.ProjectId,
                Title = dto.Title,
                Priority = dto.Priority
            });
        }

        public Task<IEnumerable<TaskResponseDto>> GetAllAsync(
            Guid ownerUserId)
        {
            Track(ownerUserId);

            return Task.FromResult<IEnumerable<TaskResponseDto>>(
                new List<TaskResponseDto>());
        }

        public Task<TaskResponseDto?> GetByIdAsync(
            Guid id, Guid ownerUserId)
        {
            Track(ownerUserId);

            return Task.FromResult<TaskResponseDto?>(null);
        }

        public Task<IEnumerable<TaskResponseDto>> GetByProjectIdAsync(
            Guid projectId, Guid ownerUserId)
        {
            Track(ownerUserId);

            return Task.FromResult<IEnumerable<TaskResponseDto>>(
                new List<TaskResponseDto>());
        }

        public Task<TaskResponseDto?> UpdateAsync(
            Guid id, UpdateTaskDto dto, Guid ownerUserId)
        {
            Track(ownerUserId);

            return Task.FromResult<TaskResponseDto?>(null);
        }

        public Task<TaskResponseDto?> StartAsync(
            Guid id, Guid ownerUserId)
        {
            Track(ownerUserId);

            return Task.FromResult<TaskResponseDto?>(null);
        }

        public Task<TaskResponseDto?> CompleteAsync(
            Guid id, Guid ownerUserId)
        {
            Track(ownerUserId);

            return Task.FromResult<TaskResponseDto?>(null);
        }

        public Task<TaskResponseDto?> CancelAsync(
            Guid id, Guid ownerUserId)
        {
            Track(ownerUserId);

            return Task.FromResult<TaskResponseDto?>(null);
        }
    }
}
