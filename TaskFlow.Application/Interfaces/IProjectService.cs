// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================
using TaskFlow.Application.DTOs.Projects;

namespace TaskFlow.Application.Interfaces;

public interface IProjectService
{
    Task<ProjectResponseDto> CreateAsync(
        CreateProjectDto dto,
        Guid ownerUserId);

    Task<IEnumerable<ProjectResponseDto>> GetAllAsync(
        Guid ownerUserId);

    Task<ProjectResponseDto?> GetByIdAsync(
        Guid id,
        Guid ownerUserId);

    Task<ProjectResponseDto?> UpdateAsync(
        Guid id,
        UpdateProjectDto dto,
        Guid ownerUserId);

    Task<bool> DeactivateAsync(
        Guid id,
        Guid ownerUserId);
}