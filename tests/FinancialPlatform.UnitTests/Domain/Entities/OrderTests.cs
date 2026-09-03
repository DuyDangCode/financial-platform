namespace FinancialPlatform.UnitTests.Domain.Entities;

using FinancialPlatform.Domain.Entities;
using FinancialPlatform.Domain.Enums;
using FinancialPlatform.Domain.Exceptions;

public class OrderTests
{
    private static readonly Guid PortfolioId = Guid.NewGuid();
    private static readonly Guid AssetId = Guid.NewGuid();

    private static Order CreateMarketOrder(decimal quantity = 10m)
        => Order.Create(PortfolioId, AssetId, OrderSide.BUY, OrderType.MARKET, quantity);

    private static Order CreateProcessingOrder(decimal quantity = 10m)
    {
        var order = CreateMarketOrder(quantity);
        order.StartProcessing();
        return order;
    }

    [Fact]
    public void Create_WithMarketOrder_SetsDefaults()
    {
        var order = CreateMarketOrder();

        Assert.Equal(PortfolioId, order.PortfolioId);
        Assert.Equal(AssetId, order.AssetId);
        Assert.Equal(OrderSide.BUY, order.Side);
        Assert.Equal(OrderType.MARKET, order.OrderType);
        Assert.Equal(10m, order.Quantity);
        Assert.Null(order.LimitPrice);
        Assert.Equal(0m, order.FilledQuantity);
        Assert.Equal(OrderStatus.PENDING, order.Status);
        Assert.Null(order.RejectionReason);
        Assert.Null(order.CompletedAt);
    }

    [Fact]
    public void Create_WithLimitOrderAndPrice_SetsLimitPrice()
    {
        var order = Order.Create(PortfolioId, AssetId, OrderSide.SELL, OrderType.LIMIT, 10m, 115.5m);

        Assert.Equal(115.5m, order.LimitPrice);
    }

    [Fact]
    public void Create_MarketOrderWithLimitPrice_StoresNullLimitPrice()
    {
        // Data model §4.9: LimitPrice is null for MARKET orders.
        var order = Order.Create(PortfolioId, AssetId, OrderSide.BUY, OrderType.MARKET, 10m, 99m);

        Assert.Null(order.LimitPrice);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveQuantity_ThrowsDomainException(decimal quantity)
    {
        var exception = Assert.Throws<DomainException>(
            () => Order.Create(PortfolioId, AssetId, OrderSide.BUY, OrderType.MARKET, quantity));

        Assert.Equal("Order quantity must be greater than zero.", exception.Message);
    }

    [Fact]
    public void Create_LimitOrderWithoutLimitPrice_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Order.Create(PortfolioId, AssetId, OrderSide.BUY, OrderType.LIMIT, 10m, null));

        Assert.Equal("Limit price is required for limit orders.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_LimitOrderWithNonPositiveLimitPrice_ThrowsDomainException(decimal limitPrice)
    {
        var exception = Assert.Throws<DomainException>(
            () => Order.Create(PortfolioId, AssetId, OrderSide.BUY, OrderType.LIMIT, 10m, limitPrice));

        Assert.Equal("Limit price must be greater than zero.", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyPortfolioId_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Order.Create(Guid.Empty, AssetId, OrderSide.BUY, OrderType.MARKET, 10m));

        Assert.Equal("Portfolio id is required.", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyAssetId_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Order.Create(PortfolioId, Guid.Empty, OrderSide.BUY, OrderType.MARKET, 10m));

        Assert.Equal("Asset id is required.", exception.Message);
    }

    [Fact]
    public void Create_WithUndefinedOrderSide_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Order.Create(PortfolioId, AssetId, (OrderSide)99, OrderType.MARKET, 10m));

        Assert.Equal("Order side is invalid.", exception.Message);
    }

    [Fact]
    public void Create_WithUndefinedOrderType_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Order.Create(PortfolioId, AssetId, OrderSide.BUY, (OrderType)99, 10m));

        Assert.Equal("Order type is invalid.", exception.Message);
    }

    [Fact]
    public void StartProcessing_OnPendingOrder_TransitionsToProcessing()
    {
        var order = CreateMarketOrder();

        order.StartProcessing();

        Assert.Equal(OrderStatus.PROCESSING, order.Status);
    }

    [Fact]
    public void StartProcessing_OnProcessingOrder_ThrowsDomainException()
    {
        var order = CreateProcessingOrder();

        var exception = Assert.Throws<DomainException>(() => order.StartProcessing());

        Assert.Equal("Only pending orders can be started.", exception.Message);
    }

    [Fact]
    public void StartProcessing_OnFilledOrder_ThrowsDomainException()
    {
        var order = CreateProcessingOrder();
        order.Fill(10m);

        var exception = Assert.Throws<DomainException>(() => order.StartProcessing());

        Assert.Equal("Only pending orders can be started.", exception.Message);
    }

    [Fact]
    public void StartProcessing_OnCancelledOrder_ThrowsDomainException()
    {
        var order = CreateMarketOrder();
        order.Cancel();

        var exception = Assert.Throws<DomainException>(() => order.StartProcessing());

        Assert.Equal("Only pending orders can be started.", exception.Message);
    }

    [Fact]
    public void StartProcessing_OnRejectedOrder_ThrowsDomainException()
    {
        var order = CreateMarketOrder();
        order.Reject("No liquidity");

        var exception = Assert.Throws<DomainException>(() => order.StartProcessing());

        Assert.Equal("Only pending orders can be started.", exception.Message);
    }

    [Fact]
    public void Cancel_OnPendingOrder_SetsCancelledAndCompletedAt()
    {
        var order = CreateMarketOrder();

        order.Cancel();

        Assert.Equal(OrderStatus.CANCELLED, order.Status);
        Assert.NotNull(order.CompletedAt);
    }

    [Fact]
    public void Cancel_OnProcessingOrder_ThrowsDomainException()
    {
        var order = CreateProcessingOrder();

        var exception = Assert.Throws<DomainException>(() => order.Cancel());

        Assert.Equal("Only pending orders can be cancelled.", exception.Message);
    }

    [Fact]
    public void Cancel_OnFilledOrder_ThrowsDomainException()
    {
        var order = CreateProcessingOrder();
        order.Fill(10m);

        var exception = Assert.Throws<DomainException>(() => order.Cancel());

        Assert.Equal("Only pending orders can be cancelled.", exception.Message);
    }

    [Fact]
    public void Cancel_OnCancelledOrder_ThrowsDomainException()
    {
        var order = CreateMarketOrder();
        order.Cancel();

        var exception = Assert.Throws<DomainException>(() => order.Cancel());

        Assert.Equal("Only pending orders can be cancelled.", exception.Message);
    }

    [Fact]
    public void Reject_OnPendingOrder_SetsRejectedWithReasonAndCompletedAt()
    {
        var order = CreateMarketOrder();

        order.Reject("Portfolio disabled");

        Assert.Equal(OrderStatus.REJECTED, order.Status);
        Assert.Equal("Portfolio disabled", order.RejectionReason);
        Assert.NotNull(order.CompletedAt);
    }

    [Fact]
    public void Reject_OnProcessingOrder_SetsRejected()
    {
        var order = CreateProcessingOrder();

        order.Reject("Engine failure");

        Assert.Equal(OrderStatus.REJECTED, order.Status);
        Assert.Equal("Engine failure", order.RejectionReason);
    }

    [Fact]
    public void Reject_OnFilledOrder_ThrowsDomainException()
    {
        var order = CreateProcessingOrder();
        order.Fill(10m);

        var exception = Assert.Throws<DomainException>(() => order.Reject("Too late"));

        Assert.Equal("Only pending or processing orders can be rejected.", exception.Message);
    }

    [Fact]
    public void Reject_OnCancelledOrder_ThrowsDomainException()
    {
        var order = CreateMarketOrder();
        order.Cancel();

        var exception = Assert.Throws<DomainException>(() => order.Reject("Too late"));

        Assert.Equal("Only pending or processing orders can be rejected.", exception.Message);
    }

    [Fact]
    public void Reject_OnRejectedOrder_ThrowsDomainException()
    {
        var order = CreateMarketOrder();
        order.Reject("First reason");

        var exception = Assert.Throws<DomainException>(() => order.Reject("Second reason"));

        Assert.Equal("Only pending or processing orders can be rejected.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Reject_WithNullOrWhiteSpaceReason_ThrowsDomainException(string? reason)
    {
        var order = CreateMarketOrder();

        var exception = Assert.Throws<DomainException>(() => order.Reject(reason!));

        Assert.Equal("Rejection reason is required.", exception.Message);
    }

    [Fact]
    public void Reject_WithReasonLongerThan256Characters_ThrowsDomainException()
    {
        var order = CreateMarketOrder();
        var reason = new string('r', 257);

        var exception = Assert.Throws<DomainException>(() => order.Reject(reason));

        Assert.Equal("Rejection reason must not exceed 256 characters.", exception.Message);
    }

    [Fact]
    public void Fill_OnProcessingOrder_FillsAndSetsCompletedAt()
    {
        var order = CreateProcessingOrder();

        order.Fill(10m);

        Assert.Equal(OrderStatus.FILLED, order.Status);
        Assert.Equal(10m, order.FilledQuantity);
        Assert.Equal(0m, order.RemainingQuantity);
        Assert.NotNull(order.CompletedAt);
    }

    [Fact]
    public void Fill_OnPendingOrder_ThrowsDomainException()
    {
        var order = CreateMarketOrder();

        var exception = Assert.Throws<DomainException>(() => order.Fill(10m));

        Assert.Equal("Only processing orders can be filled.", exception.Message);
    }

    [Fact]
    public void Fill_OnCancelledOrder_ThrowsDomainException()
    {
        var order = CreateMarketOrder();
        order.Cancel();

        var exception = Assert.Throws<DomainException>(() => order.Fill(10m));

        Assert.Equal("Only processing orders can be filled.", exception.Message);
    }

    [Fact]
    public void Fill_OnRejectedOrder_ThrowsDomainException()
    {
        var order = CreateMarketOrder();
        order.Reject("No liquidity");

        var exception = Assert.Throws<DomainException>(() => order.Fill(10m));

        Assert.Equal("Only processing orders can be filled.", exception.Message);
    }

    [Fact]
    public void Fill_OnFilledOrder_ThrowsDomainException()
    {
        var order = CreateProcessingOrder();
        order.Fill(10m);

        var exception = Assert.Throws<DomainException>(() => order.Fill(5m));

        Assert.Equal("Only processing orders can be filled.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Fill_WithNonPositiveExecutedQuantity_ThrowsDomainException(decimal executedQuantity)
    {
        var order = CreateProcessingOrder();

        var exception = Assert.Throws<DomainException>(() => order.Fill(executedQuantity));

        Assert.Equal("Executed quantity must be greater than zero.", exception.Message);
    }

    [Fact]
    public void Fill_WithExecutedQuantityExceedingRemaining_ThrowsDomainException()
    {
        var order = CreateProcessingOrder(10m);

        var exception = Assert.Throws<DomainException>(() => order.Fill(11m));

        Assert.Equal("Executed quantity cannot exceed the remaining order quantity.", exception.Message);
    }

    [Fact]
    public void Fill_WithPartialQuantity_AccumulatesFilledQuantity()
    {
        var order = CreateProcessingOrder(10m);

        order.Fill(4m);

        Assert.Equal(OrderStatus.FILLED, order.Status);
        Assert.Equal(4m, order.FilledQuantity);
        Assert.Equal(6m, order.RemainingQuantity);
    }
}