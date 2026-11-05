using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Application.Interfaces;

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

    // POST: api/projects
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("O nome é obrigatório.");

        if (dto.Name.Trim().Length > 100)
            return BadRequest(
                "O nome não pode ultrapassar 100 caracteres.");

        // Identificar o usuário autenticado pelo JWT
        var userIdClaim = User.FindFirst(
            ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");

        if (userIdClaim is null ||
            !Guid.TryParse(userIdClaim.Value, out var ownerUserId) ||
            ownerUserId == Guid.Empty)
        {
            return Unauthorized(new
            {
                message = "Token inválido: usuário não identificado."
            });
        }

        var project = await _projectService.CreateAsync(
            dto,
            ownerUserId);

        return Created(
            $"/api/projects/{project.Id}",
            project);
    }

    // GET: api/projects
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var projects = await _projectService.GetAllAsync();

        return Ok(projects);
    }

    // GET: api/projects/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var project = await _projectService.GetByIdAsync(id);

        if (project is null)
            return NotFound(new
            {
                message = "Projeto não encontrado."
            });

        return Ok(project);
    }

    // PUT: api/projects/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProjectDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("O nome é obrigatório.");

        if (dto.Name.Trim().Length > 100)
            return BadRequest(
                "O nome não pode ultrapassar 100 caracteres.");

        var project = await _projectService.UpdateAsync(id, dto);

        if (project is null)
            return NotFound(new
            {
                message = "Projeto não encontrado."
            });

        return Ok(project);
    }

    // DELETE: api/projects/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var success = await _projectService.DeactivateAsync(id);

        if (!success)
        {
            return NotFound(new
            {
                message = "Projeto não encontrado."
            });
        }

        return NoContent();
    }
}