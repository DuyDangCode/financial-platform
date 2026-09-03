namespace FinancialPlatform.UnitTests.Domain.Entities;

using FinancialPlatform.Domain.Entities;
using FinancialPlatform.Domain.Exceptions;

public class PositionTests
{
    private static readonly Guid PortfolioId = Guid.NewGuid();
    private static readonly Guid AssetId = Guid.NewGuid();

    [Fact]
    public void Create_SetsDefaults()
    {
        var position = Position.Create(PortfolioId, AssetId);

        Assert.Equal(PortfolioId, position.PortfolioId);
        Assert.Equal(AssetId, position.AssetId);
        Assert.Equal(0m, position.Quantity);
        Assert.Null(position.AverageEntryPrice);
        Assert.Equal(0m, position.RealizedPnL);
        Assert.True(position.IsClosed);
    }

    [Fact]
    public void ApplyBuy_OnEmptyPosition_SetsAverageEntryPriceToPurchasePrice()
    {
        var position = Position.Create(PortfolioId, AssetId);

        position.ApplyBuy(10m, 100m);

        Assert.Equal(10m, position.Quantity);
        Assert.Equal(100m, position.AverageEntryPrice);
        Assert.False(position.IsClosed);
    }

    [Fact]
    public void ApplyBuy_OnExistingPosition_ComputesWeightedAverage()
    {
        var position = Position.Create(PortfolioId, AssetId);
        position.ApplyBuy(10m, 100m);

        position.ApplyBuy(10m, 120m);

        Assert.Equal(20m, position.Quantity);
        Assert.Equal(110m, position.AverageEntryPrice);
    }

    [Fact]
    public void ApplyBuy_OnExistingPosition_RetainsFullDecimalPrecision()
    {
        var position = Position.Create(PortfolioId, AssetId);
        position.ApplyBuy(3m, 9.99m);

        position.ApplyBuy(1m, 10.01m);

        Assert.Equal(4m, position.Quantity);
        Assert.Equal(9.995m, position.AverageEntryPrice);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ApplyBuy_WithNonPositiveQuantity_ThrowsDomainException(decimal quantity)
    {
        var position = Position.Create(PortfolioId, AssetId);

        var exception = Assert.Throws<DomainException>(() => position.ApplyBuy(quantity, 100m));

        Assert.Equal("Buy quantity must be greater than zero.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ApplyBuy_WithNonPositivePrice_ThrowsDomainException(decimal price)
    {
        var position = Position.Create(PortfolioId, AssetId);

        var exception = Assert.Throws<DomainException>(() => position.ApplyBuy(10m, price));

        Assert.Equal("Price must be greater than zero.", exception.Message);
    }

    [Fact]
    public void ApplySell_OnPosition_DecreasesQuantityAndAccumulatesRealizedPnL()
    {
        var position = Position.Create(PortfolioId, AssetId);
        position.ApplyBuy(10m, 100m);

        position.ApplySell(4m, 120m);

        Assert.Equal(6m, position.Quantity);
        Assert.Equal(100m, position.AverageEntryPrice);
        Assert.Equal(80m, position.RealizedPnL);
    }

    [Fact]
    public void ApplySell_MultipleSells_AccumulatesRealizedPnL()
    {
        var position = Position.Create(PortfolioId, AssetId);
        position.ApplyBuy(10m, 100m);

        position.ApplySell(2m, 120m); // +40
        position.ApplySell(3m, 110m); // +30

        Assert.Equal(5m, position.Quantity);
        Assert.Equal(70m, position.RealizedPnL);
    }

    [Fact]
    public void ApplySell_SellingEntirePosition_ClosesAndResetsAverageEntryPrice()
    {
        var position = Position.Create(PortfolioId, AssetId);
        position.ApplyBuy(10m, 100m);

        position.ApplySell(10m, 130m);

        Assert.Equal(0m, position.Quantity);
        Assert.Null(position.AverageEntryPrice);
        Assert.True(position.IsClosed);
        Assert.Equal(300m, position.RealizedPnL);
    }

    [Fact]
    public void ApplySell_WithQuantityExceedingPosition_ThrowsDomainException()
    {
        var position = Position.Create(PortfolioId, AssetId);
        position.ApplyBuy(10m, 100m);

        var exception = Assert.Throws<DomainException>(() => position.ApplySell(11m, 120m));

        Assert.Equal("Cannot sell more than the current position quantity.", exception.Message);
        Assert.Equal(10m, position.Quantity);
        Assert.Equal(0m, position.RealizedPnL);
    }

    [Fact]
    public void ApplySell_OnEmptyPosition_ThrowsDomainException()
    {
        var position = Position.Create(PortfolioId, AssetId);

        var exception = Assert.Throws<DomainException>(() => position.ApplySell(5m, 100m));

        Assert.Equal("Cannot sell more than the current position quantity.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ApplySell_WithNonPositiveQuantity_ThrowsDomainException(decimal quantity)
    {
        var position = Position.Create(PortfolioId, AssetId);
        position.ApplyBuy(10m, 100m);

        var exception = Assert.Throws<DomainException>(() => position.ApplySell(quantity, 100m));

        Assert.Equal("Sell quantity must be greater than zero.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ApplySell_WithNonPositivePrice_ThrowsDomainException(decimal price)
    {
        var position = Position.Create(PortfolioId, AssetId);
        position.ApplyBuy(10m, 100m);

        var exception = Assert.Throws<DomainException>(() => position.ApplySell(1m, price));

        Assert.Equal("Price must be greater than zero.", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyPortfolioId_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Position.Create(Guid.Empty, AssetId));

        Assert.Equal("Portfolio id is required.", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyAssetId_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Position.Create(PortfolioId, Guid.Empty));

        Assert.Equal("Asset id is required.", exception.Message);
    }

    [Fact]
    public void ApplyBuy_AfterFullSell_FreshBuyRestartsAverageEntryPrice()
    {
        var position = Position.Create(PortfolioId, AssetId);
        position.ApplyBuy(10m, 100m);
        position.ApplySell(10m, 120m);

        position.ApplyBuy(20m, 150m);

        Assert.Equal(20m, position.Quantity);
        Assert.Equal(150m, position.AverageEntryPrice);
        Assert.Equal(200m, position.RealizedPnL);
    }
}