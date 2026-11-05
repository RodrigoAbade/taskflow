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

public class TaskItemDomainTests
{
    private static TaskItem NewTask() =>
        new(Guid.NewGuid(), "Tarefa", "Descrição", TaskPriority.Medium);

    [Fact]
    public void NewTask_StartsPending()
    {
        Assert.Equal(TaskState.Pending, NewTask().Status);
    }

    [Fact]
    public void EmptyProject_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new TaskItem(Guid.Empty, "T", "", TaskPriority.Low));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyTitle_Throws(string title)
    {
        Assert.Throws<ArgumentException>(() =>
            new TaskItem(Guid.NewGuid(), title, "", TaskPriority.Low));
    }

    [Fact]
    public void LongTitle_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new TaskItem(Guid.NewGuid(), new string('x', 151), "", TaskPriority.Low));
    }

    [Fact]
    public void InvalidPriority_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new TaskItem(Guid.NewGuid(), "T", "", (TaskPriority)999));
    }

    [Fact]
    public void PastDueDate_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new TaskItem(
                Guid.NewGuid(), "T", "",
                TaskPriority.Low,
                DateTime.UtcNow.AddDays(-1)));
    }

    [Fact]
    public void Start_ChangesStatus()
    {
        var task = NewTask();

        task.Start();

        Assert.Equal(TaskState.InProgress, task.Status);
    }

    [Fact]
    public void StartTwice_Throws()
    {
        var task = NewTask();
        task.Start();

        Assert.Throws<InvalidOperationException>(task.Start);
    }

    [Fact]
    public void CompleteBeforeStart_Throws()
    {
        Assert.Throws<InvalidOperationException>(NewTask().Complete);
    }

    [Fact]
    public void Complete_SetsDate()
    {
        var task = NewTask();
        task.Start();
        task.Complete();

        Assert.Equal(TaskState.Completed, task.Status);
        Assert.NotNull(task.CompletedAt);
    }

    [Fact]
    public void CancelPending_Succeeds()
    {
        var task = NewTask();

        task.Cancel();

        Assert.Equal(TaskState.Cancelled, task.Status);
    }

    [Fact]
    public void CancelCompleted_Throws()
    {
        var task = NewTask();
        task.Start();
        task.Complete();

        Assert.Throws<InvalidOperationException>(task.Cancel);
    }

    [Fact]
    public void CompleteCancelled_Throws()
    {
        var task = NewTask();
        task.Cancel();

        Assert.Throws<InvalidOperationException>(task.Complete);
    }

    [Fact]
    public void UpdateDetails_Trims()
    {
        var task = NewTask();

        task.UpdateDetails(
            " Nova ",
            " Descrição ",
            TaskPriority.High,
            null);

        Assert.Equal("Nova", task.Title);
        Assert.Equal("Descrição", task.Description);
        Assert.Equal(TaskPriority.High, task.Priority);
    }

    [Fact]
    public void UpdateCompleted_Throws()
    {
        var task = NewTask();
        task.Start();
        task.Complete();

        Assert.Throws<InvalidOperationException>(() =>
            task.UpdateDetails("Outra", "", TaskPriority.Low, null));
    }

    [Fact]
    public void Assign_StoresUser()
    {
        var task = NewTask();
        var userId = Guid.NewGuid();

        task.AssignTo(userId);

        Assert.Equal(userId, task.AssignedUserId);
    }

    [Fact]
    public void AssignEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            NewTask().AssignTo(Guid.Empty));
    }

    [Fact]
    public void AssignCancelled_Throws()
    {
        var task = NewTask();
        task.Cancel();

        Assert.Throws<InvalidOperationException>(() =>
            task.AssignTo(Guid.NewGuid()));
    }
}
