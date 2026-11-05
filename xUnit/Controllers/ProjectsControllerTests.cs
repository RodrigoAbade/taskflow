using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Controllers;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Tests.Controllers;

public class ProjectsControllerTests
{
    [Fact]
    public async Task Create_ShouldUseAuthenticatedUserId()
    {
        var userId = Guid.NewGuid();
        var service = new FakeProjectService();
        var controller = CreateController(
            service, new Claim("sub", userId.ToString()));

        var result = await controller.Create(new CreateProjectDto
        {
            Name = "Projeto de teste",
            Description = "Teste de proprietário"
        });

        Assert.IsType<CreatedResult>(result);
        Assert.True(service.CreateWasCalled);
        Assert.Equal(userId, service.ReceivedOwnerUserId);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenUserIdIsMissing()
    {
        var service = new FakeProjectService();
        var controller = CreateController(service);

        var result = await controller.Create(
            new CreateProjectDto { Name = "Projeto" });

        Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.False(service.CreateWasCalled);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenUserIdIsInvalid()
    {
        var service = new FakeProjectService();
        var controller = CreateController(
            service, new Claim("sub", "invalid-guid"));

        var result = await controller.Create(
            new CreateProjectDto { Name = "Projeto" });

        Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.False(service.CreateWasCalled);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenNameIsEmpty()
    {
        var service = new FakeProjectService();
        var controller = CreateController(
            service, new Claim("sub", Guid.NewGuid().ToString()));

        var result = await controller.Create(
            new CreateProjectDto { Name = " " });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.False(service.CreateWasCalled);
    }

    [Fact]
    public async Task GetAll_ShouldUseAuthenticatedUserId()
    {
        var userId = Guid.NewGuid();
        var service = new FakeProjectService();
        var controller = CreateController(
            service, new Claim("sub", userId.ToString()));

        var result = await controller.GetAll();

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(userId, service.ReceivedOwnerUserId);
    }

    [Fact]
    public async Task GetAll_ShouldReturnUnauthorized_WithoutUserId()
    {
        var service = new FakeProjectService();
        var controller = CreateController(service);

        var result = await controller.GetAll();

        Assert.IsType<UnauthorizedResult>(result);
        Assert.Null(service.ReceivedOwnerUserId);
    }

    [Fact]
    public async Task GetById_ShouldUseAuthenticatedUserId()
    {
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var service = new FakeProjectService();
        var controller = CreateController(
            service, new Claim("sub", userId.ToString()));

        var result = await controller.GetById(projectId);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(userId, service.ReceivedOwnerUserId);
        Assert.Equal(projectId, service.ReceivedProjectId);
    }

    [Fact]
    public async Task Update_ShouldUseAuthenticatedUserId()
    {
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var service = new FakeProjectService();
        var controller = CreateController(
            service, new Claim("sub", userId.ToString()));

        var result = await controller.Update(
            projectId,
            new UpdateProjectDto { Name = "Novo nome" });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(userId, service.ReceivedOwnerUserId);
        Assert.Equal(projectId, service.ReceivedProjectId);
    }

    [Fact]
    public async Task Deactivate_ShouldUseAuthenticatedUserId()
    {
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var service = new FakeProjectService();
        var controller = CreateController(
            service, new Claim("sub", userId.ToString()));

        var result = await controller.Deactivate(projectId);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(userId, service.ReceivedOwnerUserId);
        Assert.Equal(projectId, service.ReceivedProjectId);
    }

    [Fact]
    public async Task Deactivate_ShouldReturnUnauthorized_WithoutUserId()
    {
        var service = new FakeProjectService();
        var controller = CreateController(service);

        var result = await controller.Deactivate(Guid.NewGuid());

        Assert.IsType<UnauthorizedResult>(result);
        Assert.Null(service.ReceivedOwnerUserId);
    }

    private static ProjectsController CreateController(
        IProjectService service,
        params Claim[] claims)
    {
        var identity = new ClaimsIdentity(
            claims, authenticationType: "Test");

        return new ProjectsController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };
    }

    private sealed class FakeProjectService : IProjectService
    {
        public bool CreateWasCalled { get; private set; }
        public Guid? ReceivedOwnerUserId { get; private set; }
        public Guid? ReceivedProjectId { get; private set; }

        public Task<ProjectResponseDto> CreateAsync(
            CreateProjectDto dto, Guid ownerUserId)
        {
            CreateWasCalled = true;
            ReceivedOwnerUserId = ownerUserId;

            return Task.FromResult(new ProjectResponseDto
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description ?? string.Empty
            });
        }

        public Task<IEnumerable<ProjectResponseDto>> GetAllAsync(
            Guid ownerUserId)
        {
            ReceivedOwnerUserId = ownerUserId;

            IEnumerable<ProjectResponseDto> projects =
                new List<ProjectResponseDto>();

            return Task.FromResult(projects);
        }

        public Task<ProjectResponseDto?> GetByIdAsync(
            Guid id, Guid ownerUserId)
        {
            ReceivedProjectId = id;
            ReceivedOwnerUserId = ownerUserId;

            return Task.FromResult<ProjectResponseDto?>(
                new ProjectResponseDto { Id = id, Name = "Projeto" });
        }

        public Task<ProjectResponseDto?> UpdateAsync(
            Guid id, UpdateProjectDto dto, Guid ownerUserId)
        {
            ReceivedProjectId = id;
            ReceivedOwnerUserId = ownerUserId;

            return Task.FromResult<ProjectResponseDto?>(
                new ProjectResponseDto { Id = id, Name = dto.Name });
        }

        public Task<bool> DeactivateAsync(
            Guid id, Guid ownerUserId)
        {
            ReceivedProjectId = id;
            ReceivedOwnerUserId = ownerUserId;

            return Task.FromResult(true);
        }
    }
}