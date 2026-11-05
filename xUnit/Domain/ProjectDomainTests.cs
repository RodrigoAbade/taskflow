// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

using TaskFlow.Domain.Entities;

namespace TaskFlow.Tests.Domain;

public class ProjectDomainTests
{
    [Fact]
    public void Create_InitializesActive()
    {
        var project = new Project(
            " Projeto ",
            " Desc ",
            Guid.NewGuid());

        Assert.True(project.IsActive);
        Assert.Equal("Projeto", project.Name);
        Assert.Equal("Desc", project.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_RejectsBlankName(string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new Project(name, ""));
    }

    [Fact]
    public void Create_RejectsLongName()
    {
        Assert.Throws<ArgumentException>(() =>
            new Project(new string('x', 101), ""));
    }

    [Fact]
    public void Create_RejectsEmptyOwner()
    {
        Assert.Throws<ArgumentException>(() =>
            new Project("P", "", Guid.Empty));
    }

    [Fact]
    public void Rename_Trims()
    {
        var project = new Project("P", "");

        project.Rename(" Outro ");

        Assert.Equal("Outro", project.Name);
    }

    [Fact]
    public void Rename_RejectsBlank()
    {
        var project = new Project("P", "");

        Assert.Throws<ArgumentException>(() =>
            project.Rename(" "));
    }

    [Fact]
    public void UpdateDescription_Trims()
    {
        var project = new Project("P", "");

        project.UpdateDescription(" Nova ");

        Assert.Equal("Nova", project.Description);
    }

    [Fact]
    public void DeactivateTwice_Throws()
    {
        var project = new Project("P", "");

        project.Deactivate();

        Assert.False(project.IsActive);
        Assert.Throws<InvalidOperationException>(project.Deactivate);
    }
}
