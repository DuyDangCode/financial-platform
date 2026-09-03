namespace FinancialPlatform.UnitTests.Domain.Entities;

using FinancialPlatform.Domain.Entities;
using FinancialPlatform.Domain.Exceptions;

public class RoleTests
{
    [Fact]
    public void Create_WithValidName_SetsProperties()
    {
        var role = Role.Create("Investor");

        Assert.Equal("Investor", role.Name);
        Assert.NotEqual(Guid.Empty, role.Id);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var role = Role.Create("  Investor  ");

        Assert.Equal("Investor", role.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhiteSpaceName_ThrowsDomainException(string? name)
    {
        var exception = Assert.Throws<DomainException>(() => Role.Create(name!));

        Assert.Equal("Role name is required.", exception.Message);
    }

    [Fact]
    public void Create_WithNameLongerThan64Characters_ThrowsDomainException()
    {
        var name = new string('r', 65);

        var exception = Assert.Throws<DomainException>(() => Role.Create(name));

        Assert.Equal("Role name must not exceed 64 characters.", exception.Message);
    }
}