namespace FinancialPlatform.UnitTests.Domain.Entities;

using FinancialPlatform.Domain.Entities;
using FinancialPlatform.Domain.Exceptions;

public class AuditLogTests
{
    [Fact]
    public void Create_WithValidParameters_SetsProperties()
    {
        var actorUserId = Guid.NewGuid();
        var occurredAt = new DateTime(2026, 8, 29, 10, 30, 0, DateTimeKind.Utc);

        var log = AuditLog.Create(
            actorUserId,
            "ORDER_CANCELLED",
            "Order",
            "order-id-123",
            occurredAt,
            "192.168.1.10",
            "{\"before\":{\"status\":\"PENDING\"}}");

        Assert.Equal(actorUserId, log.ActorUserId);
        Assert.Equal("ORDER_CANCELLED", log.Action);
        Assert.Equal("Order", log.EntityType);
        Assert.Equal("order-id-123", log.EntityId);
        Assert.Equal(occurredAt, log.OccurredAt);
        Assert.Equal("192.168.1.10", log.IpAddress);
        Assert.Equal("{\"before\":{\"status\":\"PENDING\"}}", log.Details);
    }

    [Fact]
    public void Create_WithNullActorUserId_RepresentsSystemEvent()
    {
        var log = AuditLog.Create(null, "SYSTEM_STARTUP");

        Assert.Null(log.ActorUserId);
    }

    [Fact]
    public void Create_WithOptionalFieldsNull_LeavesThemNull()
    {
        var log = AuditLog.Create(Guid.NewGuid(), "USER_REGISTERED");

        Assert.Null(log.EntityType);
        Assert.Null(log.EntityId);
        Assert.Null(log.IpAddress);
        Assert.Null(log.Details);
    }

    [Fact]
    public void Create_WithoutOccurredAt_DefaultsToUtcNow()
    {
        var log = AuditLog.Create(null, "SYSTEM_STARTUP");

        Assert.True(
            (DateTime.UtcNow - log.OccurredAt).Duration() < TimeSpan.FromSeconds(5),
            "OccurredAt should default to the current UTC time.");
    }

    [Fact]
    public void Create_TrimsAction()
    {
        var log = AuditLog.Create(null, "  ORDER_CREATED  ");

        Assert.Equal("ORDER_CREATED", log.Action);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhiteSpaceAction_ThrowsDomainException(string? action)
    {
        var exception = Assert.Throws<DomainException>(() => AuditLog.Create(null, action!));

        Assert.Equal("Action is required.", exception.Message);
    }

    [Fact]
    public void Create_WithActionLongerThan64Characters_ThrowsDomainException()
    {
        var action = new string('a', 65);

        var exception = Assert.Throws<DomainException>(() => AuditLog.Create(null, action));

        Assert.Equal("Action must not exceed 64 characters.", exception.Message);
    }

    [Fact]
    public void Create_WithEntityTypeLongerThan64Characters_ThrowsDomainException()
    {
        var entityType = new string('e', 65);

        var exception = Assert.Throws<DomainException>(
            () => AuditLog.Create(null, "ORDER_CREATED", entityType));

        Assert.Equal("Entity type must not exceed 64 characters.", exception.Message);
    }

    [Fact]
    public void Create_WithEntityIdLongerThan64Characters_ThrowsDomainException()
    {
        var entityId = new string('i', 65);

        var exception = Assert.Throws<DomainException>(
            () => AuditLog.Create(null, "ORDER_CREATED", "Order", entityId));

        Assert.Equal("Entity id must not exceed 64 characters.", exception.Message);
    }

    [Fact]
    public void Create_WithIpAddressLongerThan45Characters_ThrowsDomainException()
    {
        // 45 chars is the IPv6 max length; 46 must be rejected.
        var ipAddress = new string('1', 46);

        var exception = Assert.Throws<DomainException>(
            () => AuditLog.Create(null, "USER_LOGIN", null, null, null, ipAddress));

        Assert.Equal("Ip address must not exceed 45 characters.", exception.Message);
    }

    [Fact]
    public void Create_IdRemainsZero_UntilAssignedByDatabase()
    {
        var log = AuditLog.Create(null, "ORDER_CANCELLED");

        Assert.Equal(0L, log.Id);
    }

    [Fact]
    public void AuditLog_DoesNotInheritBaseEntity_UsesLongIdentityAndNoAuditColumns()
    {
        // Physical spec (database-design §6.11): bigint identity PK, no Guid,
        // no audit columns.
        Assert.Equal(typeof(long), typeof(AuditLog).GetProperty("Id")?.PropertyType);
        Assert.Null(typeof(AuditLog).GetProperty("CreatedAt"));
        Assert.Null(typeof(AuditLog).GetProperty("UpdatedAt"));
    }
}