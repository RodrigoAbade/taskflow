using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly TaskFlowDbContext _context;

    public ProjectService(TaskFlowDbContext context)
    {
        _context = context;
    }

    // Criar projeto
    public async Task<ProjectResponseDto> CreateAsync(
        CreateProjectDto dto,
        Guid ownerUserId)
    {
        var project = new Project(
            dto.Name,
            dto.Description ?? string.Empty,
            ownerUserId);

        _context.Projects.Add(project);

        await _context.SaveChangesAsync();

        return MapToDto(project);
    }

    // Listar todos os projetos
    public async Task<IEnumerable<ProjectResponseDto>> GetAllAsync()
    {
        var projects = await _context.Projects
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();

        return projects.Select(MapToDto).ToList();
    }

    // Buscar projeto pelo ID
    public async Task<ProjectResponseDto?> GetByIdAsync(Guid id)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project is null)
            return null;

        return MapToDto(project);
    }

    // Atualizar projeto
    public async Task<ProjectResponseDto?> UpdateAsync(
        Guid id,
        UpdateProjectDto dto)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project is null)
            return null;

        project.Rename(dto.Name);

        project.UpdateDescription(
            dto.Description ?? string.Empty);

        await _context.SaveChangesAsync();

        return MapToDto(project);
    }

    // Desativar projeto
    public async Task<bool> DeactivateAsync(Guid id)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project is null)
            return false;

        project.Deactivate();

        await _context.SaveChangesAsync();

        return true;
    }

    // Converter entidade para DTO
    private static ProjectResponseDto MapToDto(Project project)
    {
        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            IsActive = project.IsActive
        };
    }
}