namespace FinancialPlatform.Domain.Entities;

using FinancialPlatform.Domain.Exceptions;

public class Role : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    private Role() { }

    private Role(string name)
    {
        SetName(name);
    }

    public static Role Create(string name)
        => new(name);

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Role name is required.");

        if (name.Length > 64)
            throw new DomainException("Role name must not exceed 64 characters.");

        Name = name.Trim();
    }
}