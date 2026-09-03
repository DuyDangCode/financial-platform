namespace FinancialPlatform.Domain.Entities;

using FinancialPlatform.Domain.Enums;
using FinancialPlatform.Domain.Exceptions;

public class Order : BaseEntity
{
    public Guid PortfolioId { get; private set; }
    public Guid AssetId { get; private set; }
    public OrderSide Side { get; private set; }
    public OrderType OrderType { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal? LimitPrice { get; private set; }
    public decimal FilledQuantity { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.PENDING;
    public string? RejectionReason { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    /// <summary>Remaining unfilled units (BR-017 support).</summary>
    public decimal RemainingQuantity => Quantity - FilledQuantity;

    private Order() { }

    private Order(
        Guid portfolioId,
        Guid assetId,
        OrderSide side,
        OrderType orderType,
        decimal quantity,
        decimal? limitPrice)
    {
        SetPortfolioId(portfolioId);
        SetAssetId(assetId);
        SetSide(side);
        SetOrderType(orderType);
        SetQuantity(quantity);

        // A LIMIT order requires a positive limit price (BR-011); a MARKET
        // order never carries one (data-model §4.9: "null for MARKET").
        if (orderType == OrderType.LIMIT)
        {
            SetLimitPrice(limitPrice);
        }
    }

    public static Order Create(
        Guid portfolioId,
        Guid assetId,
        OrderSide side,
        OrderType orderType,
        decimal quantity,
        decimal? limitPrice = null)
        => new(portfolioId, assetId, side, orderType, quantity, limitPrice);

    /// <summary>PENDING → PROCESSING. The engine picked the order up.</summary>
    public void StartProcessing()
    {
        if (Status != OrderStatus.PENDING)
            throw new DomainException("Only pending orders can be started.");

        Status = OrderStatus.PROCESSING;
    }

    /// <summary>PENDING → CANCELLED (BR-012, FR-014). Terminal.</summary>
    public void Cancel()
    {
        if (Status != OrderStatus.PENDING)
            throw new DomainException("Only pending orders can be cancelled.");

        Status = OrderStatus.CANCELLED;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>PENDING or PROCESSING → REJECTED. Terminal.</summary>
    public void Reject(string reason)
    {
        if (Status is not (OrderStatus.PENDING or OrderStatus.PROCESSING))
            throw new DomainException("Only pending or processing orders can be rejected.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Rejection reason is required.");

        if (reason.Length > 256)
            throw new DomainException("Rejection reason must not exceed 256 characters.");

        RejectionReason = reason.Trim();
        Status = OrderStatus.REJECTED;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// PROCESSING → FILLED. Accumulates FilledQuantity and enforces BR-017
    /// (aggregate executed quantity never exceeds the order quantity). The
    /// execution price is recorded on the Execution entity, not here — the
    /// Order aggregate has no price attribute in the data model. The MVP
    /// application flow fills with the full quantity (one execution per order).
    /// </summary>
    public void Fill(decimal executedQuantity)
    {
        if (Status != OrderStatus.PROCESSING)
            throw new DomainException("Only processing orders can be filled.");

        if (executedQuantity <= 0)
            throw new DomainException("Executed quantity must be greater than zero.");

        if (executedQuantity > RemainingQuantity)
            throw new DomainException("Executed quantity cannot exceed the remaining order quantity.");

        FilledQuantity += executedQuantity;
        Status = OrderStatus.FILLED;
        CompletedAt = DateTime.UtcNow;
    }

    private void SetPortfolioId(Guid portfolioId)
    {
        if (portfolioId == Guid.Empty)
            throw new DomainException("Portfolio id is required.");

        PortfolioId = portfolioId;
    }

    private void SetAssetId(Guid assetId)
    {
        if (assetId == Guid.Empty)
            throw new DomainException("Asset id is required.");

        AssetId = assetId;
    }

    private void SetSide(OrderSide side)
    {
        if (!Enum.IsDefined(side))
            throw new DomainException("Order side is invalid.");

        Side = side;
    }

    private void SetOrderType(OrderType orderType)
    {
        if (!Enum.IsDefined(orderType))
            throw new DomainException("Order type is invalid.");

        OrderType = orderType;
    }

    private void SetQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Order quantity must be greater than zero.");

        Quantity = quantity;
    }

    private void SetLimitPrice(decimal? limitPrice)
    {
        if (limitPrice is null)
            throw new DomainException("Limit price is required for limit orders.");

        if (limitPrice <= 0)
            throw new DomainException("Limit price must be greater than zero.");

        LimitPrice = limitPrice;
    }
}