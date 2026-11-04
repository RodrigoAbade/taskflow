
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.Users;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly TaskFlowDbContext _context;

    public UserService(TaskFlowDbContext context)
    {
        _context = context;
    }

    // Cadastrar usuário
    public async Task<UserResponseDto> CreateAsync(
        CreateUserDto dto)
    {
        var email = dto.Email?.Trim() ?? string.Empty;

        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == email);

        if (emailExists)
            throw new InvalidOperationException(
                "Este e-mail já está cadastrado.");

        var user = new User(dto.Name, email);

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    // Listar todos os usuários
    public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
    {
        var users = await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync();

        return users.Select(MapToDto).ToList();
    }

    // Buscar usuário pelo ID
    public async Task<UserResponseDto?> GetByIdAsync(Guid id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        return user is null ? null : MapToDto(user);
    }

    // Desativar usuário
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

    // Converter entidade para DTO
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
