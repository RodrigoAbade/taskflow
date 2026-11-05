
using TaskFlow.Application.DTOs.Tasks;

namespace TaskFlow.Application.Interfaces;

public interface ITaskService
{
    Task<TaskResponseDto> CreateAsync(
        CreateTaskDto dto, Guid ownerUserId);

    Task<IEnumerable<TaskResponseDto>> GetAllAsync(
        Guid ownerUserId);

    Task<TaskResponseDto?> GetByIdAsync(
        Guid id, Guid ownerUserId);

    Task<IEnumerable<TaskResponseDto>> GetByProjectIdAsync(
        Guid projectId, Guid ownerUserId);

    Task<TaskResponseDto?> UpdateAsync(
        Guid id, UpdateTaskDto dto, Guid ownerUserId);

    Task<TaskResponseDto?> StartAsync(
        Guid id, Guid ownerUserId);

    Task<TaskResponseDto?> CompleteAsync(
        Guid id, Guid ownerUserId);

    Task<TaskResponseDto?> CancelAsync(
        Guid id, Guid ownerUserId);
}
