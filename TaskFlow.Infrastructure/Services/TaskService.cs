// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

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

    private static void ValidateOwner(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty)
            throw new ArgumentException(
                "Usuário proprietário inválido.");
    }

    private IQueryable<TaskItem> OwnedTasks(
        Guid ownerUserId)
    {
        ValidateOwner(ownerUserId);

        return _context.Tasks.Where(t =>
            _context.Projects.Any(p =>
                p.Id == t.ProjectId &&
                p.OwnerUserId == ownerUserId));
    }

    public async Task<TaskResponseDto> CreateAsync(
        CreateTaskDto dto, Guid ownerUserId)
    {
        ValidateOwner(ownerUserId);

        var projectExists = await _context.Projects
            .AnyAsync(p =>
                p.Id == dto.ProjectId &&
                p.OwnerUserId == ownerUserId &&
                p.IsActive);

        if (!projectExists)
            throw new KeyNotFoundException(
                "Projeto não encontrado.");

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

    public async Task<IEnumerable<TaskResponseDto>> GetAllAsync(
        Guid ownerUserId)
    {
        var tasks = await OwnedTasks(ownerUserId)
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToDto).ToList();
    }

    public async Task<TaskResponseDto?> GetByIdAsync(
        Guid id, Guid ownerUserId)
    {
        var task = await OwnedTasks(ownerUserId)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        return task is null ? null : MapToDto(task);
    }

    public async Task<IEnumerable<TaskResponseDto>>
        GetByProjectIdAsync(
            Guid projectId, Guid ownerUserId)
    {
        var tasks = await OwnedTasks(ownerUserId)
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToDto).ToList();
    }

    public async Task<TaskResponseDto?> UpdateAsync(
        Guid id,
        UpdateTaskDto dto,
        Guid ownerUserId)
    {
        var task = await OwnedTasks(ownerUserId)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task is null)
            return null;

        task.UpdateDetails(
            dto.Title,
            dto.Description,
            dto.Priority,
            dto.DueDate);

        await _context.SaveChangesAsync();

        return MapToDto(task);
    }

    public async Task<TaskResponseDto?> StartAsync(
        Guid id, Guid ownerUserId)
    {
        var task = await OwnedTasks(ownerUserId)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task is null)
            return null;

        task.Start();

        await _context.SaveChangesAsync();

        return MapToDto(task);
    }

    public async Task<TaskResponseDto?> CompleteAsync(
        Guid id, Guid ownerUserId)
    {
        var task = await OwnedTasks(ownerUserId)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task is null)
            return null;

        task.Complete();

        await _context.SaveChangesAsync();

        return MapToDto(task);
    }

    public async Task<TaskResponseDto?> CancelAsync(
        Guid id, Guid ownerUserId)
    {
        var task = await OwnedTasks(ownerUserId)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task is null)
            return null;

        task.Cancel();

        await _context.SaveChangesAsync();

        return MapToDto(task);
    }

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
