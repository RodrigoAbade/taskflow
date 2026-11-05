// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.DTOs.Users;
using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly TaskFlowDbContext _context;
    private readonly IPasswordService _passwordService;

    public AuthService(
        TaskFlowDbContext context,
        IPasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    public async Task<UserResponseDto?> LoginAsync(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
            return null;

        var email = dto.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);

        // Não permite login de usuários inexistentes ou desativados
        if (user is null || !user.IsActive)
            return null;

        // Usuários antigos podem não ter senha cadastrada
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            return null;

        // Verifica a senha informada
        var validPassword = _passwordService.VerifyPassword(
            dto.Password,
            user.PasswordHash);

        if (!validPassword)
            return null;

        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };
    }
}
