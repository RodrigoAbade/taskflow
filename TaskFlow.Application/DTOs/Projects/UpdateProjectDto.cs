// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

namespace TaskFlow.Application.DTOs.Projects;

public class UpdateProjectDto
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
