
using TaskFlow.Application.DTOs.Projects;

namespace TaskFlow.Application.Interfaces;

public interface IProjectService
{
    Task<ProjectResponseDto> CreateAsync(
        CreateProjectDto dto);

    Task<IEnumerable<ProjectResponseDto>> GetAllAsync();

    Task<ProjectResponseDto?> GetByIdAsync(Guid id);
}
