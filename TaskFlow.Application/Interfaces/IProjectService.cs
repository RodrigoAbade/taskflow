using TaskFlow.Application.DTOs.Projects;

namespace TaskFlow.Application.Interfaces;

public interface IProjectService
{
    Task<ProjectResponseDto> CreateAsync(
        CreateProjectDto dto,
        Guid ownerUserId);

    Task<IEnumerable<ProjectResponseDto>> GetAllAsync();

    Task<ProjectResponseDto?> GetByIdAsync(Guid id);

    Task<ProjectResponseDto?> UpdateAsync(
        Guid id,
        UpdateProjectDto dto);

    Task<bool> DeactivateAsync(Guid id);
}