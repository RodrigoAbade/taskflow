
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs.Tasks;

public class CreateTaskDto
{
    public Guid ProjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TaskPriority Priority { get; set; }

    public DateTime? DueDate { get; set; }
}
