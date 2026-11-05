// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

using TaskFlow.Domain.Entities;

namespace TaskFlow.Tests.Domain;

public class UserDomainTests
{
    [Fact]
    public void Create_NormalizesEmailAndName()
    {
        var user = new User(
            " Pessoa ",
            " USER@EXAMPLE.COM ");

        Assert.Equal("Pessoa", user.Name);
        Assert.Equal("user@example.com", user.Email);
        Assert.True(user.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_RejectsBlankName(string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new User(name, "a@example.com"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Create_RejectsInvalidEmail(string email)
    {
        Assert.Throws<ArgumentException>(() =>
            new User("Nome", email));
    }

    [Fact]
    public void UpdateName_Trims()
    {
        var user = new User("A", "a@example.com");

        user.UpdateName(" B ");

        Assert.Equal("B", user.Name);
    }

    [Fact]
    public void UpdateName_RejectsBlank()
    {
        var user = new User("A", "a@example.com");

        Assert.Throws<ArgumentException>(() =>
            user.UpdateName(" "));
    }

    [Fact]
    public void PasswordHash_Required()
    {
        var user = new User("A", "a@example.com");

        Assert.Throws<ArgumentException>(() =>
            user.SetPasswordHash(" "));
    }

    [Fact]
    public void PasswordHash_IsStored()
    {
        var user = new User("A", "a@example.com");

        user.SetPasswordHash("hash");

        Assert.Equal("hash", user.PasswordHash);
    }

    [Fact]
    public void DeactivateTwice_Throws()
    {
        var user = new User("A", "a@example.com");

        user.Deactivate();

        Assert.False(user.IsActive);
        Assert.Throws<InvalidOperationException>(user.Deactivate);
    }
}
