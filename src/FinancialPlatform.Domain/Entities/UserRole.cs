namespace FinancialPlatform.Domain.Entities;

using FinancialPlatform.Domain.Exceptions;

/// <summary>
/// Join entity between users and roles.
/// Intentionally does NOT inherit <see cref="BaseEntity"/>: the physical table
/// (database-design §6.4) defines a composite primary key (UserId, RoleId) and
/// has no Guid Id or audit columns of its own.
/// </summary>
public class UserRole
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public DateTime AssignedAt { get; private set; }

    private UserRole() { }

    private UserRole(Guid userId, Guid roleId)
    {
        SetUserId(userId);
        SetRoleId(roleId);
        AssignedAt = DateTime.UtcNow;
    }

    public static UserRole Create(Guid userId, Guid roleId)
        => new(userId, roleId);

    private void SetUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User id is required.");

        UserId = userId;
    }

    private void SetRoleId(Guid roleId)
    {
        if (roleId == Guid.Empty)
            throw new DomainException("Role id is required.");

        RoleId = roleId;
    }
}