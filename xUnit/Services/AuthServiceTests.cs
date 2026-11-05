
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.Tests.Services;

public class AuthServiceTests
{
    private static TaskFlowDbContext Db()
    {
        var options = new DbContextOptionsBuilder<TaskFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TaskFlowDbContext(options);
    }

    [Fact]
    public async Task ValidCredentials_LoginSucceeds()
    {
        await using var db = Db();
        var passwords = new PasswordService();

        var user = new User("Test", "test@example.com");
        user.SetPasswordHash(passwords.HashPassword("Password123!"));

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var service = new AuthService(db, passwords);

        var result = await service.LoginAsync(new LoginDto
        {
            Email = " TEST@EXAMPLE.COM ",
            Password = "Password123!"
        });

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task WrongPassword_LoginFails()
    {
        await using var db = Db();
        var passwords = new PasswordService();

        var user = new User("Test", "test@example.com");
        user.SetPasswordHash(passwords.HashPassword("Password123!"));

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var service = new AuthService(db, passwords);

        var result = await service.LoginAsync(new LoginDto
        {
            Email = "test@example.com",
            Password = "wrong"
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task UnknownUser_LoginFails()
    {
        await using var db = Db();

        var service = new AuthService(db, new PasswordService());

        var result = await service.LoginAsync(new LoginDto
        {
            Email = "missing@example.com",
            Password = "Password123!"
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task InactiveUser_LoginFails()
    {
        await using var db = Db();
        var passwords = new PasswordService();

        var user = new User("Test", "test@example.com");
        user.SetPasswordHash(passwords.HashPassword("Password123!"));
        user.Deactivate();

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var service = new AuthService(db, passwords);

        var result = await service.LoginAsync(new LoginDto
        {
            Email = "test@example.com",
            Password = "Password123!"
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task NoPasswordHash_LoginFails()
    {
        await using var db = Db();

        db.Users.Add(new User("Test", "test@example.com"));
        await db.SaveChangesAsync();

        var service = new AuthService(db, new PasswordService());

        var result = await service.LoginAsync(new LoginDto
        {
            Email = "test@example.com",
            Password = "Password123!"
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task BlankCredentials_LoginFails()
    {
        await using var db = Db();

        var service = new AuthService(db, new PasswordService());

        var result = await service.LoginAsync(new LoginDto
        {
            Email = "",
            Password = ""
        });

        Assert.Null(result);
    }
}
