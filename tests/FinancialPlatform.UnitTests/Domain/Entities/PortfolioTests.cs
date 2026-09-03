namespace FinancialPlatform.UnitTests.Domain.Entities;

using FinancialPlatform.Domain.Entities;
using FinancialPlatform.Domain.Enums;
using FinancialPlatform.Domain.Exceptions;

public class PortfolioTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidParameters_SetsPropertiesWithDefaults()
    {
        var portfolio = Portfolio.Create(UserId, "Growth");

        Assert.Equal(UserId, portfolio.UserId);
        Assert.Equal("Growth", portfolio.Name);
        Assert.Equal("USD", portfolio.BaseCurrency);
        Assert.Equal(PortfolioStatus.ACTIVE, portfolio.Status);
        Assert.Null(portfolio.Notes);
        Assert.NotEqual(Guid.Empty, portfolio.Id);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var portfolio = Portfolio.Create(UserId, "  Growth  ");

        Assert.Equal("Growth", portfolio.Name);
    }

    [Fact]
    public void Create_WithCustomBaseCurrency_NormalizesToUpperInvariant()
    {
        var portfolio = Portfolio.Create(UserId, "Growth", "eur");

        Assert.Equal("EUR", portfolio.BaseCurrency);
    }

    [Fact]
    public void Create_WithNullBaseCurrency_UsesUsdDefault()
    {
        var portfolio = Portfolio.Create(UserId, "Growth", null!);

        Assert.Equal("USD", portfolio.BaseCurrency);
    }

    [Fact]
    public void Create_WithInvalidCurrencyLength_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Portfolio.Create(UserId, "Growth", "US"));

        Assert.Equal("Currency must be a 3-letter ISO 4217 code.", exception.Message);
    }

    [Fact]
    public void Create_WithNotes_SetsNotes()
    {
        var portfolio = Portfolio.Create(UserId, "Growth", "USD", "Retirement savings");

        Assert.Equal("Retirement savings", portfolio.Notes);
    }

    [Fact]
    public void Create_WithNotesLongerThan512Characters_ThrowsDomainException()
    {
        var notes = new string('n', 513);

        var exception = Assert.Throws<DomainException>(
            () => Portfolio.Create(UserId, "Growth", "USD", notes));

        Assert.Equal("Notes must not exceed 512 characters.", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyUserId_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Portfolio.Create(Guid.Empty, "Growth"));

        Assert.Equal("User id is required.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhiteSpaceName_ThrowsDomainException(string? name)
    {
        var exception = Assert.Throws<DomainException>(
            () => Portfolio.Create(UserId, name!));

        Assert.Equal("Portfolio name is required.", exception.Message);
    }

    [Fact]
    public void Create_WithNameLongerThan128Characters_ThrowsDomainException()
    {
        var name = new string('p', 129);

        var exception = Assert.Throws<DomainException>(
            () => Portfolio.Create(UserId, name));

        Assert.Equal("Portfolio name must not exceed 128 characters.", exception.Message);
    }

    [Fact]
    public void Rename_WithValidName_UpdatesName()
    {
        var portfolio = Portfolio.Create(UserId, "Growth");

        portfolio.Rename("Long Term");

        Assert.Equal("Long Term", portfolio.Name);
    }

    [Fact]
    public void Rename_WithEmptyName_ThrowsDomainException()
    {
        var portfolio = Portfolio.Create(UserId, "Growth");

        var exception = Assert.Throws<DomainException>(() => portfolio.Rename(""));

        Assert.Equal("Portfolio name is required.", exception.Message);
    }

    [Fact]
    public void Activate_OnDisabledPortfolio_SetsStatusActive()
    {
        var portfolio = Portfolio.Create(UserId, "Growth");
        portfolio.Disable();

        portfolio.Activate();

        Assert.Equal(PortfolioStatus.ACTIVE, portfolio.Status);
    }

    [Fact]
    public void Disable_ActivePortfolio_SetsStatusDisabled()
    {
        var portfolio = Portfolio.Create(UserId, "Growth");

        portfolio.Disable();

        Assert.Equal(PortfolioStatus.DISABLED, portfolio.Status);
    }
}