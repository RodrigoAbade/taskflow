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
        ValidateOwnerUserId(ownerUserId);

        var project = new Project(
            dto.Name,
            dto.Description ?? string.Empty,
            ownerUserId);

        _context.Projects.Add(project);

        await _context.SaveChangesAsync();

        return MapToDto(project);
    }

    // Listar somente projetos do usuário autenticado
    public async Task<IEnumerable<ProjectResponseDto>> GetAllAsync(
        Guid ownerUserId)
    {
        ValidateOwnerUserId(ownerUserId);

        var projects = await _context.Projects
            .AsNoTracking()
            .Where(p => p.OwnerUserId == ownerUserId)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return projects.Select(MapToDto).ToList();
    }

    // Buscar projeto pertencente ao usuário
    public async Task<ProjectResponseDto?> GetByIdAsync(
        Guid id,
        Guid ownerUserId)
    {
        ValidateOwnerUserId(ownerUserId);

        var project = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.OwnerUserId == ownerUserId);

        if (project is null)
            return null;

        return MapToDto(project);
    }

    // Atualizar somente projeto do proprietário
    public async Task<ProjectResponseDto?> UpdateAsync(
        Guid id,
        UpdateProjectDto dto,
        Guid ownerUserId)
    {
        ValidateOwnerUserId(ownerUserId);

        var project = await _context.Projects
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.OwnerUserId == ownerUserId);

        if (project is null)
            return null;

        project.Rename(dto.Name);

        project.UpdateDescription(
            dto.Description ?? string.Empty);

        await _context.SaveChangesAsync();

        return MapToDto(project);
    }

    // Desativar somente projeto do proprietário
    public async Task<bool> DeactivateAsync(
        Guid id,
        Guid ownerUserId)
    {
        ValidateOwnerUserId(ownerUserId);

        var project = await _context.Projects
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.OwnerUserId == ownerUserId);

        if (project is null)
            return false;

        project.Deactivate();

        await _context.SaveChangesAsync();

        return true;
    }

    // Validar identificação do proprietário
    private static void ValidateOwnerUserId(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "O usuário proprietário é inválido.",
                nameof(ownerUserId));
        }
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