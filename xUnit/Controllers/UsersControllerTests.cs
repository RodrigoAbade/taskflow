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
using TaskFlow.Application.DTOs.Users;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Tests.Controllers;

public class UsersControllerTests
{
    [Fact]
    public async Task Deactivate_ShouldForbid_WhenUserTargetsAnotherAccount()
    {
        var authenticatedUserId =
            Guid.Parse("11111111-1111-1111-1111-111111111111");

        var otherUserId =
            Guid.Parse("22222222-2222-2222-2222-222222222222");

        var service = new FakeUserService();
        var controller = CreateController(service, authenticatedUserId);

        var result = await controller.Deactivate(otherUserId);

        Assert.IsType<ForbidResult>(result);
        Assert.False(service.DeactivateWasCalled);
        Assert.Null(service.DeactivatedUserId);
    }

    [Fact]
    public async Task Deactivate_ShouldReturnNoContent_WhenUserTargetsOwnAccount()
    {
        var userId = Guid.NewGuid();

        var service = new FakeUserService();
        var controller = CreateController(service, userId);

        var result = await controller.Deactivate(userId);

        Assert.IsType<NoContentResult>(result);
        Assert.True(service.DeactivateWasCalled);
        Assert.Equal(userId, service.DeactivatedUserId);
    }

    [Fact]
    public async Task GetById_ShouldForbid_WhenUserRequestsAnotherAccount()
    {
        var authenticatedUserId =
            Guid.Parse("11111111-1111-1111-1111-111111111111");

        var otherUserId =
            Guid.Parse("22222222-2222-2222-2222-222222222222");

        var service = new FakeUserService();
        var controller = CreateController(service, authenticatedUserId);

        var result = await controller.GetById(otherUserId);

        Assert.IsType<ForbidResult>(result);
        Assert.False(service.GetByIdWasCalled);
        Assert.Null(service.RequestedUserId);
    }

    [Fact]
    public async Task GetById_ShouldReturnUser_WhenUserRequestsOwnAccount()
    {
        var userId = Guid.NewGuid();

        var expectedUser = new UserResponseDto
        {
            Id = userId,
            Name = "Usuário de Teste",
            Email = "consulta@example.com",
            IsActive = true
        };

        var service = new FakeUserService
        {
            UserToReturn = expectedUser
        };

        var controller = CreateController(service, userId);

        var result = await controller.GetById(userId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUser = Assert.IsType<UserResponseDto>(okResult.Value);

        Assert.Same(expectedUser, returnedUser);
        Assert.True(service.GetByIdWasCalled);
        Assert.Equal(userId, service.RequestedUserId);
    }

    private static UsersController CreateController(
        IUserService service,
        Guid authenticatedUserId)
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim("sub", authenticatedUserId.ToString())
            },
            authenticationType: "Test");

        return new UsersController(service)
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

    private sealed class FakeUserService : IUserService
    {
        public bool DeactivateWasCalled { get; private set; }

        public Guid? DeactivatedUserId { get; private set; }

        public bool GetByIdWasCalled { get; private set; }

        public Guid? RequestedUserId { get; private set; }

        public UserResponseDto? UserToReturn { get; set; }

        public Task<bool> DeactivateAsync(Guid id)
        {
            DeactivateWasCalled = true;
            DeactivatedUserId = id;

            return Task.FromResult(true);
        }

        public Task<UserResponseDto?> GetByIdAsync(Guid id)
        {
            GetByIdWasCalled = true;
            RequestedUserId = id;

            return Task.FromResult(UserToReturn);
        }

        public Task<UserResponseDto> CreateAsync(CreateUserDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserResponseDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }
    }
}