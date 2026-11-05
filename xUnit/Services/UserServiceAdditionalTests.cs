
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.Users;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.Tests.Services;

public class UserServiceAdditionalTests
{
    private static TaskFlowDbContext Db()
    {
        var options = new DbContextOptionsBuilder<TaskFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TaskFlowDbContext(options);
    }

    [Fact]
    public async Task Create_StoresHashNotPlainPassword()
    {
        await using var db = Db();

        var service = new UserService(db, new PasswordService());

        var dto = new CreateUserDto
        {
            Name = "Test",
            Email = "TEST@EXAMPLE.COM",
            Password = "Password123!"
        };

        var result = await service.CreateAsync(dto);
        var stored = await db.Users.SingleAsync();

        Assert.Equal("test@example.com", result.Email);
        Assert.NotEqual(dto.Password, stored.PasswordHash);

        Assert.True(
            new PasswordService().VerifyPassword(
                dto.Password,
                stored.PasswordHash!));
    }

    [Fact]
    public async Task DuplicateEmail_Throws()
    {
        await using var db = Db();

        var service = new UserService(db, new PasswordService());

        await service.CreateAsync(new CreateUserDto
        {
            Name = "A",
            Email = "a@example.com",
            Password = "Password123!"
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateUserDto
            {
                Name = "B",
                Email = " A@EXAMPLE.COM ",
                Password = "Password123!"
            }));
    }

    [Fact]
    public async Task DeactivateMissing_ReturnsFalse()
    {
        await using var db = Db();

        var service = new UserService(db, new PasswordService());

        Assert.False(
            await service.DeactivateAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeactivateExisting_ReturnsTrue()
    {
        await using var db = Db();

        var service = new UserService(db, new PasswordService());

        var user = await service.CreateAsync(new CreateUserDto
        {
            Name = "A",
            Email = "a@example.com",
            Password = "Password123!"
        });

        Assert.True(await service.DeactivateAsync(user.Id));

        var stored = await service.GetByIdAsync(user.Id);

        Assert.NotNull(stored);
        Assert.False(stored.IsActive);
    }
}
