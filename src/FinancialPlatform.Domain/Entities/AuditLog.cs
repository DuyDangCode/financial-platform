namespace FinancialPlatform.Domain.Entities;

using FinancialPlatform.Domain.Exceptions;

/// <summary>
/// Append-only record of business and security events.
/// Intentionally does NOT inherit <see cref="BaseEntity"/>: the physical table
/// (database-design §6.11) uses a bigint identity primary key and carries no
/// Guid or audit columns. Id is assigned by the database; it remains 0 until
/// then. Details represents the jsonb payload as a pre-serialized JSON string
/// (or null when omitted).
/// </summary>
public class AuditLog
{
    public long Id { get; private set; }
    public Guid? ActorUserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string? EntityType { get; private set; }
    public string? EntityId { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? Details { get; private set; }

    private AuditLog() { }

    private AuditLog(
        Guid? actorUserId,
        string action,
        string? entityType,
        string? entityId,
        DateTime occurredAt,
        string? ipAddress,
        string? details)
    {
        ActorUserId = actorUserId;
        SetAction(action);
        SetEntityType(entityType);
        SetEntityId(entityId);
        OccurredAt = occurredAt;
        SetIpAddress(ipAddress);
        Details = details;
    }

    public static AuditLog Create(
        Guid? actorUserId,
        string action,
        string? entityType = null,
        string? entityId = null,
        DateTime? occurredAt = null,
        string? ipAddress = null,
        string? details = null)
        => new(actorUserId, action, entityType, entityId, occurredAt ?? DateTime.UtcNow, ipAddress, details);

    private void SetAction(string action)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new DomainException("Action is required.");

        if (action.Length > 64)
            throw new DomainException("Action must not exceed 64 characters.");

        Action = action.Trim();
    }

    private void SetEntityType(string? entityType)
    {
        if (entityType is not null && entityType.Length > 64)
            throw new DomainException("Entity type must not exceed 64 characters.");

        EntityType = entityType;
    }

    private void SetEntityId(string? entityId)
    {
        if (entityId is not null && entityId.Length > 64)
            throw new DomainException("Entity id must not exceed 64 characters.");

        EntityId = entityId;
    }

    private void SetIpAddress(string? ipAddress)
    {
        if (ipAddress is not null && ipAddress.Length > 45)
            throw new DomainException("Ip address must not exceed 45 characters.");

        IpAddress = ipAddress;
    }
}