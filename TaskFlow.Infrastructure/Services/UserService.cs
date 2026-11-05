// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.Users;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly TaskFlowDbContext _context;
    private readonly IPasswordService _passwordService;

    public UserService(
        TaskFlowDbContext context,
        IPasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
    {
        var email = dto.Email?.Trim().ToLowerInvariant()
            ?? string.Empty;

        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == email);

        if (emailExists)
            throw new InvalidOperationException(
                "Este e-mail já está cadastrado.");

        var user = new User(dto.Name, email);

        // Gera o hash seguro da senha
        var passwordHash = _passwordService.HashPassword(
            dto.Password);

        // Armazena somente o hash
        user.SetPasswordHash(passwordHash);

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
    {
        var users = await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync();

        return users.Select(MapToDto).ToList();
    }

    public async Task<UserResponseDto?> GetByIdAsync(Guid id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        return user is null ? null : MapToDto(user);
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return false;

        user.Deactivate();

        await _context.SaveChangesAsync();

        return true;
    }

    private static UserResponseDto MapToDto(User user)
    {
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
