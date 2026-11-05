// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Application.Interfaces;
using TaskFlow.Api.Helpers;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    private Guid? GetAuthenticatedUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");

        if (claim is null ||
            !Guid.TryParse(claim.Value, out var userId) ||
            userId == Guid.Empty)
        {
            return null;
        }

        return userId;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return ApiErrors.BadRequest(
                "O nome é obrigatório.");

        if (dto.Name.Trim().Length > 100)
            return ApiErrors.BadRequest(
                "O nome não pode ultrapassar 100 caracteres.");

        var userId = GetAuthenticatedUserId();

        if (userId is null)
            return ApiErrors.Unauthorized(
                "Token inválido: usuário não identificado.");

        var project = await _projectService.CreateAsync(
            dto, userId.Value);

        return Created($"/api/projects/{project.Id}", project);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
            return Unauthorized();

        var projects = await _projectService.GetAllAsync(
            userId.Value);

        return Ok(projects);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
            return Unauthorized();

        var project = await _projectService.GetByIdAsync(
            id, userId.Value);

        if (project is null)
            return ApiErrors.NotFound(
                "Projeto não encontrado.");

        return Ok(project);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProjectDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return ApiErrors.BadRequest(
                "O nome é obrigatório.");

        if (dto.Name.Trim().Length > 100)
            return ApiErrors.BadRequest(
                "O nome não pode ultrapassar 100 caracteres.");

        var userId = GetAuthenticatedUserId();

        if (userId is null)
            return Unauthorized();

        var project = await _projectService.UpdateAsync(
            id, dto, userId.Value);

        if (project is null)
            return ApiErrors.NotFound(
                "Projeto não encontrado.");

        return Ok(project);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
            return Unauthorized();

        var success = await _projectService.DeactivateAsync(
            id, userId.Value);

        if (!success)
            return ApiErrors.NotFound(
                "Projeto não encontrado.");

        return NoContent();
    }
}
