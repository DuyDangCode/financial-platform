namespace FinancialPlatform.UnitTests.Domain.Entities;

using FinancialPlatform.Domain.Entities;
using FinancialPlatform.Domain.Enums;
using FinancialPlatform.Domain.Exceptions;

public class TransactionTests
{
    private static readonly Guid OrderId = Guid.NewGuid();
    private static readonly Guid ExecutionId = Guid.NewGuid();
    private static readonly Guid PortfolioId = Guid.NewGuid();
    private static readonly Guid AssetId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidParameters_SetsPropertiesAndComputesGrossAmount()
    {
        var transaction = Transaction.Create(
            OrderId,
            ExecutionId,
            PortfolioId,
            AssetId,
            OrderSide.BUY,
            10m,
            100m);

        Assert.Equal(OrderId, transaction.OrderId);
        Assert.Equal(ExecutionId, transaction.ExecutionId);
        Assert.Equal(PortfolioId, transaction.PortfolioId);
        Assert.Equal(AssetId, transaction.AssetId);
        Assert.Equal(OrderSide.BUY, transaction.Side);
        Assert.Equal(10m, transaction.Quantity);
        Assert.Equal(100m, transaction.Price);
        Assert.Equal(1000m, transaction.GrossAmount);
        Assert.Equal(0m, transaction.Fee);
        Assert.True(
            (DateTime.UtcNow - transaction.ExecutedAt).Duration() < TimeSpan.FromSeconds(5),
            "ExecutedAt should default to the current UTC time.");
    }

    [Fact]
    public void Create_WithFractionalQuantity_ComputesGrossAmount()
    {
        var transaction = Transaction.Create(
            OrderId,
            ExecutionId,
            PortfolioId,
            AssetId,
            OrderSide.SELL,
            2.5m,
            10.5m);

        Assert.Equal(26.25m, transaction.GrossAmount);
    }

    [Fact]
    public void Create_WithFeeAndExecutedAt_UsesProvidedValues()
    {
        var executedAt = new DateTime(2026, 8, 29, 12, 0, 0, DateTimeKind.Utc);

        var transaction = Transaction.Create(
            OrderId,
            ExecutionId,
            PortfolioId,
            AssetId,
            OrderSide.BUY,
            10m,
            100m,
            1.25m,
            executedAt);

        Assert.Equal(1.25m, transaction.Fee);
        Assert.Equal(executedAt, transaction.ExecutedAt);
    }

    [Fact]
    public void Create_WithEmptyOrderId_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Transaction.Create(Guid.Empty, ExecutionId, PortfolioId, AssetId, OrderSide.BUY, 10m, 100m));

        Assert.Equal("Order id is required.", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyExecutionId_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Transaction.Create(OrderId, Guid.Empty, PortfolioId, AssetId, OrderSide.BUY, 10m, 100m));

        Assert.Equal("Execution id is required.", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyPortfolioId_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Transaction.Create(OrderId, ExecutionId, Guid.Empty, AssetId, OrderSide.BUY, 10m, 100m));

        Assert.Equal("Portfolio id is required.", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyAssetId_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Transaction.Create(OrderId, ExecutionId, PortfolioId, Guid.Empty, OrderSide.BUY, 10m, 100m));

        Assert.Equal("Asset id is required.", exception.Message);
    }

    [Fact]
    public void Create_WithUndefinedSide_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Transaction.Create(OrderId, ExecutionId, PortfolioId, AssetId, (OrderSide)99, 10m, 100m));

        Assert.Equal("Order side is invalid.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveQuantity_ThrowsDomainException(decimal quantity)
    {
        var exception = Assert.Throws<DomainException>(
            () => Transaction.Create(OrderId, ExecutionId, PortfolioId, AssetId, OrderSide.BUY, quantity, 100m));

        Assert.Equal("Transaction quantity must be greater than zero.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositivePrice_ThrowsDomainException(decimal price)
    {
        var exception = Assert.Throws<DomainException>(
            () => Transaction.Create(OrderId, ExecutionId, PortfolioId, AssetId, OrderSide.BUY, 10m, price));

        Assert.Equal("Transaction price must be greater than zero.", exception.Message);
    }

    [Fact]
    public void Create_WithNegativeFee_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Transaction.Create(OrderId, ExecutionId, PortfolioId, AssetId, OrderSide.BUY, 10m, 100m, -1m));

        Assert.Equal("Fee must not be negative.", exception.Message);
    }
}