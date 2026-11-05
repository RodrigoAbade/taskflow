// ============================================================
// TaskFlow API
// Autor: Rodrigo Abade Nunes
// GitHub: https://github.com/RodrigoAbade
// Projeto: https://github.com/RodrigoAbade/taskflow
// ============================================================

using Microsoft.AspNetCore.Identity;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException(
                "A senha é obrigatória.",
                nameof(password));

        return _passwordHasher.HashPassword(null!, password);
    }

    public bool VerifyPassword(
        string password,
        string passwordHash)
    {
        if (string.IsNullOrEmpty(password) ||
            string.IsNullOrWhiteSpace(passwordHash))
            return false;

        var result = _passwordHasher.VerifyHashedPassword(
            null!,
            passwordHash,
            password);

        return result != PasswordVerificationResult.Failed;
    }
}
