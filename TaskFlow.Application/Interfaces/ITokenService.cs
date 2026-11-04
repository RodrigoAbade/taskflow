
using TaskFlow.Application.DTOs.Users;

namespace TaskFlow.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(UserResponseDto user);
}
