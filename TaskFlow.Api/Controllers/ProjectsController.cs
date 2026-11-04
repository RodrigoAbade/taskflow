
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly TaskFlowDbContext _context;

    public ProjectsController(TaskFlowDbContext context)
    {
        _context = context;
    }

    // POST: api/projects
    // Cadastrar um novo projeto
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("O nome é obrigatório.");

        if (request.Name.Trim().Length > 100)
            return BadRequest(
                "O nome não pode ultrapassar 100 caracteres.");

        var project = new Project(
            request.Name,
            request.Description ?? string.Empty);

        _context.Projects.Add(project);

        await _context.SaveChangesAsync();

        return Created(
            $"/api/projects/{project.Id}",
            new
            {
                project.Id,
                project.Name,
                project.Description,
                project.CreatedAt,
                project.IsActive
            });
    }

    // GET: api/projects
    // Listar todos os projetos
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var projects = await _context.Projects
            .AsNoTracking()
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.CreatedAt,
                p.IsActive
            })
            .ToListAsync();

        return Ok(projects);
    }


[HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.CreatedAt,
                p.IsActive
            })
            .FirstOrDefaultAsync();

        if (project is null)
            return NotFound(new
            {
                message = "Projeto não encontrado."
            });

        return Ok(project);
    } 
}



public record CreateProjectRequest(
    string Name,
    string? Description);
