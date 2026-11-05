// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskState = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Tests.Domain;

public class TaskItemTests
{
    [Fact]
    public void NewTask_ShouldStartAsPending()
    {
        // Arrange & Act
        var task = new TaskItem(
            Guid.NewGuid(),
            "Criar API",
            "Implementar endpoints",
            TaskPriority.High
        );

        // Assert
        Assert.Equal(TaskState.Pending, task.Status);
    }

    [Fact]
    public void Complete_ShouldThrow_WhenTaskIsPending()
    {
        // Arrange
        var task = new TaskItem(
            Guid.NewGuid(),
            "Criar API",
            "Implementar endpoints",
            TaskPriority.Medium
        );

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => task.Complete()
        );
    }

    [Fact]
    public void Start_ShouldChangeStatusToInProgress()
    {
        // Arrange
        var task = new TaskItem(
            Guid.NewGuid(),
            "Criar API",
            "Implementar endpoints",
            TaskPriority.High
        );

        // Act
        task.Start();

        // Assert
        Assert.Equal(TaskState.InProgress, task.Status);
    }

    [Fact]
    public void Complete_ShouldSetCompletedAt()
    {
        // Arrange
        var task = new TaskItem(
            Guid.NewGuid(),
            "Criar API",
            "Implementar endpoints",
            TaskPriority.High
        );

        // Act
        task.Start();
        task.Complete();

        // Assert
        Assert.Equal(TaskState.Completed, task.Status);
        Assert.NotNull(task.CompletedAt);
    }

    [Fact]
    public void Cancel_ShouldThrow_WhenTaskIsCompleted()
    {
        // Arrange
        var task = new TaskItem(
            Guid.NewGuid(),
            "Criar API",
            "Implementar endpoints",
            TaskPriority.High
        );

        task.Start();
        task.Complete();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => task.Cancel()
        );
    }
}
