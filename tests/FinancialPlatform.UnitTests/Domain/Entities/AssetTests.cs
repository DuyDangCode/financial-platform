namespace FinancialPlatform.UnitTests.Domain.Entities;

using FinancialPlatform.Domain.Entities;
using FinancialPlatform.Domain.Enums;
using FinancialPlatform.Domain.Exceptions;

public class AssetTests
{
    [Fact]
    public void Create_WithValidParameters_SetsPropertiesWithDefaults()
    {
        var asset = Asset.Create("aapl", "Apple Inc.", AssetType.STOCK, 189.84m);

        Assert.Equal("AAPL", asset.Symbol);
        Assert.Equal("Apple Inc.", asset.Name);
        Assert.Equal(AssetType.STOCK, asset.AssetType);
        Assert.Equal(189.84m, asset.CurrentPrice);
        Assert.Equal("USD", asset.Currency);
        Assert.Equal(AssetStatus.ACTIVE, asset.Status);
        Assert.True(
            (DateTime.UtcNow - asset.PriceUpdatedAt).Duration() < TimeSpan.FromSeconds(5),
            "PriceUpdatedAt should default to the current UTC time.");
    }

    [Fact]
    public void Create_NormalizesSymbolToUppercaseAndTrims()
    {
        var asset = Asset.Create("  spy  ", "SPDR S&P 500 ETF", AssetType.ETF, 532.10m);

        Assert.Equal("SPY", asset.Symbol);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var asset = Asset.Create("MSFT", "  Microsoft Corp  ", AssetType.STOCK, 415.55m);

        Assert.Equal("Microsoft Corp", asset.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhiteSpaceSymbol_ThrowsDomainException(string? symbol)
    {
        var exception = Assert.Throws<DomainException>(
            () => Asset.Create(symbol!, "Apple Inc.", AssetType.STOCK, 189.84m));

        Assert.Equal("Asset symbol is required.", exception.Message);
    }

    [Fact]
    public void Create_WithSymbolLongerThan16Characters_ThrowsDomainException()
    {
        var symbol = new string('s', 17);

        var exception = Assert.Throws<DomainException>(
            () => Asset.Create(symbol, "Long Symbol", AssetType.STOCK, 1m));

        Assert.Equal("Asset symbol must not exceed 16 characters.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhiteSpaceName_ThrowsDomainException(string? name)
    {
        var exception = Assert.Throws<DomainException>(
            () => Asset.Create("AAPL", name!, AssetType.STOCK, 189.84m));

        Assert.Equal("Asset name is required.", exception.Message);
    }

    [Fact]
    public void Create_WithNameLongerThan256Characters_ThrowsDomainException()
    {
        var name = new string('a', 257);

        var exception = Assert.Throws<DomainException>(
            () => Asset.Create("AAPL", name, AssetType.STOCK, 189.84m));

        Assert.Equal("Asset name must not exceed 256 characters.", exception.Message);
    }

    [Fact]
    public void Create_WithNegativeCurrentPrice_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Asset.Create("AAPL", "Apple Inc.", AssetType.STOCK, -1m));

        Assert.Equal("Asset current price must not be negative.", exception.Message);
    }

    [Fact]
    public void Create_WithInvalidAssetType_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Asset.Create("AAPL", "Apple Inc.", (AssetType)99, 189.84m));

        Assert.Equal("Asset type is invalid.", exception.Message);
    }

    [Fact]
    public void Create_WithCustomCurrency_NormalizesToUpperInvariant()
    {
        var asset = Asset.Create("AAPL", "Apple Inc.", AssetType.STOCK, 189.84m, "eur");

        Assert.Equal("EUR", asset.Currency);
    }

    [Fact]
    public void RefreshPrice_WithValidPrice_UpdatesPriceAndTimestamp()
    {
        var asset = Asset.Create("AAPL", "Apple Inc.", AssetType.STOCK, 189.84m);
        var originalTimestamp = asset.PriceUpdatedAt;

        asset.RefreshPrice(190.25m);

        Assert.Equal(190.25m, asset.CurrentPrice);
        Assert.True(asset.PriceUpdatedAt >= originalTimestamp);
    }

    [Fact]
    public void RefreshPrice_WithNegativePrice_ThrowsDomainException()
    {
        var asset = Asset.Create("AAPL", "Apple Inc.", AssetType.STOCK, 189.84m);

        var exception = Assert.Throws<DomainException>(() => asset.RefreshPrice(-5m));

        Assert.Equal("Asset current price must not be negative.", exception.Message);
    }

    [Fact]
    public void Activate_OnInactiveAsset_SetsStatusActive()
    {
        var asset = Asset.Create("AAPL", "Apple Inc.", AssetType.STOCK, 189.84m);
        asset.Deactivate();

        asset.Activate();

        Assert.Equal(AssetStatus.ACTIVE, asset.Status);
    }

    [Fact]
    public void Deactivate_ActiveAsset_SetsStatusInactive()
    {
        var asset = Asset.Create("AAPL", "Apple Inc.", AssetType.STOCK, 189.84m);

        asset.Deactivate();

        Assert.Equal(AssetStatus.INACTIVE, asset.Status);
    }
}