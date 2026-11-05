// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

using TaskFlow.Application.DTOs.Users;

namespace TaskFlow.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(UserResponseDto user);
}
