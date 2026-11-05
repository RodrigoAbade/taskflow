// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

using TaskFlow.Application.DTOs.Users;

namespace TaskFlow.Application.Interfaces;

public interface IUserService
{
    // Cadastrar usuário
    Task<UserResponseDto> CreateAsync(
        CreateUserDto dto);

    // Listar todos os usuários
    Task<IEnumerable<UserResponseDto>> GetAllAsync();

    // Buscar usuário pelo ID
    Task<UserResponseDto?> GetByIdAsync(Guid id);

    // Desativar usuário
    Task<bool> DeactivateAsync(Guid id);
}
