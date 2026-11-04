
namespace TaskFlow.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string? PasswordHash { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private User()
    {
        Name = string.Empty;
        Email = string.Empty;
    }

    public User(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "O nome do usuário é obrigatório.",
                nameof(name));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "O e-mail é obrigatório.",
                nameof(email));

        if (name.Trim().Length > 100)
            throw new ArgumentException(
                "O nome não pode ultrapassar 100 caracteres.",
                nameof(name));

        if (!System.Net.Mail.MailAddress.TryCreate(
                email.Trim(), out var address)
            || address.Address != email.Trim())
            throw new ArgumentException(
                "O e-mail informado é inválido.",
                nameof(email));

        Id = Guid.NewGuid();
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = null;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException(
                "O nome não pode ser vazio.",
                nameof(newName));

        if (newName.Trim().Length > 100)
            throw new ArgumentException(
                "O nome não pode ultrapassar 100 caracteres.",
                nameof(newName));

        Name = newName.Trim();
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException(
                "O hash da senha é obrigatório.",
                nameof(passwordHash));

        PasswordHash = passwordHash;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException(
                "O usuário já está desativado.");

        IsActive = false;
    }
}
