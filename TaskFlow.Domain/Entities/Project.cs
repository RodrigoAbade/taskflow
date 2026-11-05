namespace TaskFlow.Domain.Entities;

public class Project
{
    public Guid Id { get; private set; }
    public Guid? OwnerUserId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private Project()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    public Project(
        string name,
        string description,
        Guid? ownerUserId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "O nome do projeto é obrigatório.",
                nameof(name));

        if (name.Length > 100)
            throw new ArgumentException(
                "O nome não pode ultrapassar 100 caracteres.",
                nameof(name));

        if (ownerUserId == Guid.Empty)
            throw new ArgumentException(
                "O usuário proprietário é inválido.",
                nameof(ownerUserId));

        Id = Guid.NewGuid();
        OwnerUserId = ownerUserId;
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException(
                "O nome do projeto é obrigatório.",
                nameof(newName));

        if (newName.Trim().Length > 100)
            throw new ArgumentException(
                "O nome não pode ultrapassar 100 caracteres.",
                nameof(newName));

        Name = newName.Trim();
    }

    public void UpdateDescription(string description)
    {
        Description = description?.Trim() ?? string.Empty;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException(
                "O projeto já está desativado.");

        IsActive = false;
    }
}