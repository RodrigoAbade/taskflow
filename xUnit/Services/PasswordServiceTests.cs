// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

using TaskFlow.Infrastructure.Services;

namespace TaskFlow.Tests.Services;

public class PasswordServiceTests
{
    [Fact]
    public void Hash_IsNotPlaintext()
    {
        var service = new PasswordService();

        var hash = service.HashPassword("Password123!");

        Assert.NotEqual("Password123!", hash);
        Assert.True(service.VerifyPassword("Password123!", hash));
    }

    [Fact]
    public void WrongPassword_IsRejected()
    {
        var service = new PasswordService();
        var hash = service.HashPassword("Password123!");

        Assert.False(service.VerifyPassword("wrong", hash));
    }

    [Fact]
    public void DifferentSalts_ProduceDifferentHashes()
    {
        var service = new PasswordService();

        var hash1 = service.HashPassword("Password123!");
        var hash2 = service.HashPassword("Password123!");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void BlankPassword_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new PasswordService().HashPassword(" "));
    }

    [Fact]
    public void BlankHash_IsRejected()
    {
        Assert.False(
            new PasswordService().VerifyPassword("password", ""));
    }
}
