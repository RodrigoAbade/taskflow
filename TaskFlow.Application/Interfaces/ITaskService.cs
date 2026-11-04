
using TaskFlow.Application.DTOs.Tasks;

namespace TaskFlow.Application.Interfaces;

public interface ITaskService
{
    // Criar tarefa
    Task<TaskResponseDto> CreateAsync(
        CreateTaskDto dto);

    // Listar todas as tarefas
    Task<IEnumerable<TaskResponseDto>> GetAllAsync();

    // Buscar tarefa pelo ID
    Task<TaskResponseDto?> GetByIdAsync(Guid id);

    // Listar tarefas de um projeto
    Task<IEnumerable<TaskResponseDto>> GetByProjectIdAsync(
        Guid projectId);

    // Atualizar tarefa
    Task<TaskResponseDto?> UpdateAsync(
        Guid id,
        UpdateTaskDto dto);

    // Iniciar tarefa
    Task<TaskResponseDto?> StartAsync(Guid id);

    // Concluir tarefa
    Task<TaskResponseDto?> CompleteAsync(Guid id);

    // Cancelar tarefa
    Task<TaskResponseDto?> CancelAsync(Guid id);
}
