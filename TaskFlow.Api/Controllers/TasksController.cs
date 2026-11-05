// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Application.Interfaces;
using TaskFlow.Api.Helpers;

namespace TaskFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
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
        [FromBody] CreateTaskDto dto)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
            return Unauthorized();

        if (dto.ProjectId == Guid.Empty)
            return ApiErrors.BadRequest(
                "O projeto é obrigatório.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return ApiErrors.BadRequest(
                "O título é obrigatório.");

        try
        {
            var task = await _taskService.CreateAsync(
                dto, userId.Value);

            return CreatedAtAction(
                nameof(GetById),
                new { id = task.Id },
                task);
        }
        catch (KeyNotFoundException)
        {
            return ApiErrors.NotFound(
                "Projeto não encontrado.");
        }
        catch (ArgumentException ex)
        {
            return ApiErrors.BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
            return Unauthorized();

        var tasks = await _taskService.GetAllAsync(
            userId.Value);

        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
            return Unauthorized();

        var task = await _taskService.GetByIdAsync(
            id, userId.Value);

        if (task is null)
            return ApiErrors.NotFound(
                "Tarefa não encontrada.");

        return Ok(task);
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<IActionResult> GetByProjectId(
        Guid projectId)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
            return Unauthorized();

        var tasks = await _taskService.GetByProjectIdAsync(
            projectId, userId.Value);

        return Ok(tasks);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTaskDto dto)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.Title))
            return ApiErrors.BadRequest(
                "O título é obrigatório.");

        if (dto.Title.Trim().Length > 150)
            return ApiErrors.BadRequest(
                "O título não pode ultrapassar 150 caracteres.");

        try
        {
            var task = await _taskService.UpdateAsync(
                id, dto, userId.Value);

            if (task is null)
                return ApiErrors.NotFound(
                    "Tarefa não encontrada.");

            return Ok(task);
        }
        catch (ArgumentException ex)
        {
            return ApiErrors.BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ApiErrors.Conflict(ex.Message);
        }
    }

    [HttpPatch("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
            return Unauthorized();

        try
        {
            var task = await _taskService.StartAsync(
                id, userId.Value);

            if (task is null)
                return ApiErrors.NotFound(
                    "Tarefa não encontrada.");

            return Ok(task);
        }
        catch (InvalidOperationException ex)
        {
            return ApiErrors.Conflict(ex.Message);
        }
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
            return Unauthorized();

        try
        {
            var task = await _taskService.CompleteAsync(
                id, userId.Value);

            if (task is null)
                return ApiErrors.NotFound(
                    "Tarefa não encontrada.");

            return Ok(task);
        }
        catch (InvalidOperationException ex)
        {
            return ApiErrors.Conflict(ex.Message);
        }
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
            return Unauthorized();

        try
        {
            var task = await _taskService.CancelAsync(
                id, userId.Value);

            if (task is null)
                return ApiErrors.NotFound(
                    "Tarefa não encontrada.");

            return Ok(task);
        }
        catch (InvalidOperationException ex)
        {
            return ApiErrors.Conflict(ex.Message);
        }
    }
}
