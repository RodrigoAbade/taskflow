
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Helpers;
using TaskFlow.Application.DTOs.Users;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // POST: api/users
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return ApiErrors.BadRequest(
                "O nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(dto.Email))
            return ApiErrors.BadRequest(
                "O e-mail é obrigatório.");

        try
        {
            var user = await _userService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = user.Id },
                user);
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

    // GET: api/users/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userIdClaim = User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(
            userIdClaim,
            out var authenticatedUserId))
        {
            return Unauthorized();
        }

        if (id != authenticatedUserId)
            return Forbid();

        var user = await _userService.GetByIdAsync(id);

        if (user is null)
            return ApiErrors.NotFound(
                "Usuário não encontrado.");

        return Ok(user);
    }

    // DELETE: api/users/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var userIdClaim = User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(
            userIdClaim,
            out var authenticatedUserId))
        {
            return Unauthorized();
        }

        if (id != authenticatedUserId)
            return Forbid();

        try
        {
            var success = await _userService.DeactivateAsync(id);

            if (!success)
                return ApiErrors.NotFound(
                    "Usuário não encontrado.");

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return ApiErrors.Conflict(ex.Message);
        }
    }
}
