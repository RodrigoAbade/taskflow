
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Interfaces;
using TaskFlow.Api.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public AuthController(
        IAuthService authService,
        ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    // POST: api/auth/login
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto)
    {
        var user = await _authService.LoginAsync(dto);

        if (user is null)
        {
            return ApiErrors.Unauthorized(
                "E-mail ou senha inválidos.");
        }

        var token = _tokenService.GenerateToken(user);

        return Ok(new
        {
            message = "Login realizado com sucesso.",
            token,
            tokenType = "Bearer",
            expiresIn = 3600,
            user
        });
    }
}
