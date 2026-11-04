
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly TaskFlowDbContext _context;

    public TaskService(TaskFlowDbContext context)
    {
        _context = context;
    }

    // Criar tarefa
    public async Task<TaskResponseDto> CreateAsync(
        CreateTaskDto dto)
    {
        var projectExists = await _context.Projects
            .AnyAsync(p => p.Id == dto.ProjectId && p.IsActive);

        if (!projectExists)
            throw new ArgumentException(
                "Projeto não encontrado ou inativo.");

        var task = new TaskItem(
            dto.ProjectId,
            dto.Title,
            dto.Description,
            dto.Priority,
            dto.DueDate);

        _context.Tasks.Add(task);

        await _context.SaveChangesAsync();

        return MapToDto(task);
    }

    // Listar todas as tarefas
    public async Task<IEnumerable<TaskResponseDto>> GetAllAsync()
    {
        var tasks = await _context.Tasks
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToDto).ToList();
    }

    // Buscar tarefa pelo ID
    public async Task<TaskResponseDto?> GetByIdAsync(Guid id)
    {
        var task = await _context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        return task is null ? null : MapToDto(task);
    }

    // Listar tarefas de um projeto
    public async Task<IEnumerable<TaskResponseDto>> GetByProjectIdAsync(
        Guid projectId)
    {
        var tasks = await _context.Tasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToDto).ToList();
    }

    public async Task<TaskResponseDto?> UpdateAsync(
        Guid id,
        UpdateTaskDto dto)
    {
        // Buscar a tarefa no banco de dados
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id);

        // Verificar se a tarefa existe
        if (task is null)
            return null;

        // Aplicar as regras de negócio
        task.UpdateDetails(
            dto.Title,
            dto.Description,
            dto.Priority,
            dto.DueDate);

        // Salvar as alterações no SQL Server
        await _context.SaveChangesAsync();

        // Retornar a tarefa atualizada
        return MapToDto(task);
    }


    // Iniciar uma tarefa
    public async Task<TaskResponseDto?> StartAsync(Guid id)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task is null)
            return null;

        task.Start();

        await _context.SaveChangesAsync();

        return MapToDto(task);
    }

    // Concluir uma tarefa
    public async Task<TaskResponseDto?> CompleteAsync(Guid id)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task is null)
            return null;

        task.Complete();

        await _context.SaveChangesAsync();

        return MapToDto(task);
    }

    // Cancelar uma tarefa
    public async Task<TaskResponseDto?> CancelAsync(Guid id)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task is null)
            return null;

        task.Cancel();

        await _context.SaveChangesAsync();

        return MapToDto(task);
    }

    // Converter entidade para DTO
    private static TaskResponseDto MapToDto(TaskItem task)
    {
        return new TaskResponseDto
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            AssignedUserId = task.AssignedUserId,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            CreatedAt = task.CreatedAt,
            DueDate = task.DueDate,
            CompletedAt = task.CompletedAt
        };
    }
}
