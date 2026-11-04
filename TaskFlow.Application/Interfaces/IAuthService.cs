
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.DTOs.Users;

namespace TaskFlow.Application.Interfaces;

public interface IAuthService
{
    Task<UserResponseDto?> LoginAsync(LoginDto dto);
}
