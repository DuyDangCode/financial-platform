namespace FinancialPlatform.UnitTests.Domain.Entities;

using FinancialPlatform.Domain.Entities;
using FinancialPlatform.Domain.Exceptions;

public class UserRoleTests
{
    [Fact]
    public void Create_WithValidIds_SetsProperties()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var userRole = UserRole.Create(userId, roleId);

        Assert.Equal(userId, userRole.UserId);
        Assert.Equal(roleId, userRole.RoleId);
        Assert.True(
            (DateTime.UtcNow - userRole.AssignedAt).Duration() < TimeSpan.FromSeconds(5),
            "AssignedAt should default to the current UTC time.");
    }

    [Fact]
    public void Create_WithEmptyUserId_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => UserRole.Create(Guid.Empty, Guid.NewGuid()));

        Assert.Equal("User id is required.", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyRoleId_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => UserRole.Create(Guid.NewGuid(), Guid.Empty));

        Assert.Equal("Role id is required.", exception.Message);
    }

    [Fact]
    public void UserRole_DoesNotInheritBaseEntity_ExposesNoIdOrAuditColumns()
    {
        // Physical spec (database-design §6.4): composite PK (UserId, RoleId),
        // no Guid Id column and no audit columns.
        Assert.Null(typeof(UserRole).GetProperty("Id"));
        Assert.Null(typeof(UserRole).GetProperty("CreatedAt"));
        Assert.Null(typeof(UserRole).GetProperty("UpdatedAt"));
    }
}