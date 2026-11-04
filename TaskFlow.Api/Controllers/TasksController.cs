
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    // POST: api/tasks
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTaskDto dto)
    {
        if (dto.ProjectId == Guid.Empty)
            return BadRequest("O projeto é obrigatório.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("O título é obrigatório.");

        try
        {
            var task = await _taskService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = task.Id },
                task);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/tasks
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tasks = await _taskService.GetAllAsync();

        return Ok(tasks);
    }

    // GET: api/tasks/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var task = await _taskService.GetByIdAsync(id);

        if (task is null)
            return NotFound(new
            {
                message = "Tarefa não encontrada."
            });

        return Ok(task);
    }

    // GET: api/tasks/project/{projectId}
    [HttpGet("project/{projectId:guid}")]
    public async Task<IActionResult> GetByProjectId(
        Guid projectId)
    {
        var tasks = await _taskService
            .GetByProjectIdAsync(projectId);

        return Ok(tasks);
    }

    // PUT: api/tasks/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTaskDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new
            {
                message = "O título é obrigatório."
            });

        if (dto.Title.Trim().Length > 150)
            return BadRequest(new
            {
                message = "O título não pode ultrapassar 150 caracteres."
            });

        try
        {
            var task = await _taskService.UpdateAsync(id, dto);

            if (task is null)
                return NotFound(new
                {
                    message = "Tarefa não encontrada."
                });

            return Ok(task);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // NOVO: Iniciar tarefa
    // PATCH: api/tasks/{id}/start
    [HttpPatch("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        try
        {
            var task = await _taskService.StartAsync(id);

            if (task is null)
                return NotFound(new
                {
                    message = "Tarefa não encontrada."
                });

            return Ok(task);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // NOVO: Concluir tarefa
    // PATCH: api/tasks/{id}/complete
    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        try
        {
            var task = await _taskService.CompleteAsync(id);

            if (task is null)
                return NotFound(new
                {
                    message = "Tarefa não encontrada."
                });

            return Ok(task);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // NOVO: Cancelar tarefa
    // PATCH: api/tasks/{id}/cancel
    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            var task = await _taskService.CancelAsync(id);

            if (task is null)
                return NotFound(new
                {
                    message = "Tarefa não encontrada."
                });

            return Ok(task);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
