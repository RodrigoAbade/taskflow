
using TaskFlow.Domain.Enums;
using TaskState = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid? AssignedUserId { get; private set; }

    public string Title { get; private set; }
    public string Description { get; private set; }

    public TaskState Status { get; private set; }
    public TaskPriority Priority { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private TaskItem()
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    public TaskItem(
        Guid projectId,
        string title,
        string description,
        TaskPriority priority,
        DateTime? dueDate = null)
    {
        if (projectId == Guid.Empty)
            throw new ArgumentException("Projeto inválido.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("O título é obrigatório.");

        if (title.Trim().Length > 150)
            throw new ArgumentException("O título é muito longo.");

        if (!Enum.IsDefined(priority))
            throw new ArgumentException("Prioridade inválida.");

        if (dueDate.HasValue && dueDate.Value <= DateTime.UtcNow)
            throw new ArgumentException("O prazo deve ser futuro.");

        Id = Guid.NewGuid();
        ProjectId = projectId;
        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Priority = priority;
        Status = TaskState.Pending;
        CreatedAt = DateTime.UtcNow;
        DueDate = dueDate;
    }

    // NOVO: Atualizar informações da tarefa
    public void UpdateDetails(
        string title,
        string description,
        TaskPriority priority,
        DateTime? dueDate)
    {
        if (Status is TaskState.Completed or TaskState.Cancelled)
            throw new InvalidOperationException(
                "Não é possível editar uma tarefa encerrada.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("O título é obrigatório.");

        if (title.Trim().Length > 150)
            throw new ArgumentException("O título é muito longo.");

        if (!Enum.IsDefined(priority))
            throw new ArgumentException("Prioridade inválida.");

        if (dueDate.HasValue && dueDate.Value <= DateTime.UtcNow)
            throw new ArgumentException("O prazo deve ser futuro.");

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Priority = priority;
        DueDate = dueDate;
    }

    public void AssignTo(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Usuário inválido.");

        if (Status is TaskState.Completed or TaskState.Cancelled)
            throw new InvalidOperationException(
                "Não é possível atribuir uma tarefa encerrada.");

        AssignedUserId = userId;
    }

    public void Start()
    {
        if (Status != TaskState.Pending)
            throw new InvalidOperationException(
                "Apenas tarefas pendentes podem ser iniciadas.");

        Status = TaskState.InProgress;
    }

    public void Complete()
    {
        if (Status != TaskState.InProgress)
            throw new InvalidOperationException(
                "A tarefa precisa estar em andamento.");

        Status = TaskState.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is TaskState.Completed or TaskState.Cancelled)
            throw new InvalidOperationException(
                "Não é possível cancelar uma tarefa encerrada.");

        Status = TaskState.Cancelled;
    }
}
