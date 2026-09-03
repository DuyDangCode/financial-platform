namespace FinancialPlatform.Domain.Entities;

using FinancialPlatform.Domain.Enums;
using FinancialPlatform.Domain.Exceptions;

/// <summary>
/// Immutable financial fact produced by a successful execution. Created once
/// via <see cref="Create"/> and never mutated (BR-026). GrossAmount is a
/// snapshot computed as Quantity × Price at creation and never recomputed
/// (BR-025). Denormalized copies are taken at creation (data-model §4.11).
/// </summary>
public class Transaction : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ExecutionId { get; private set; }
    public Guid PortfolioId { get; private set; }
    public Guid AssetId { get; private set; }
    public OrderSide Side { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal Price { get; private set; }
    public decimal GrossAmount { get; private set; }
    public decimal Fee { get; private set; }
    public DateTime ExecutedAt { get; private set; }

    private Transaction() { }

    private Transaction(
        Guid orderId,
        Guid executionId,
        Guid portfolioId,
        Guid assetId,
        OrderSide side,
        decimal quantity,
        decimal price,
        decimal fee,
        DateTime executedAt)
    {
        SetOrderId(orderId);
        SetExecutionId(executionId);
        SetPortfolioId(portfolioId);
        SetAssetId(assetId);
        SetSide(side);
        SetQuantity(quantity);
        SetPrice(price);
        SetFee(fee);
        GrossAmount = quantity * price;
        ExecutedAt = executedAt;
    }

    public static Transaction Create(
        Guid orderId,
        Guid executionId,
        Guid portfolioId,
        Guid assetId,
        OrderSide side,
        decimal quantity,
        decimal price,
        decimal fee = 0,
        DateTime? executedAt = null)
        => new(orderId, executionId, portfolioId, assetId, side, quantity, price, fee, executedAt ?? DateTime.UtcNow);

    private void SetOrderId(Guid orderId)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("Order id is required.");

        OrderId = orderId;
    }

    private void SetExecutionId(Guid executionId)
    {
        if (executionId == Guid.Empty)
            throw new DomainException("Execution id is required.");

        ExecutionId = executionId;
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

    private void SetQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Transaction quantity must be greater than zero.");

        Quantity = quantity;
    }

    private void SetPrice(decimal price)
    {
        if (price <= 0)
            throw new DomainException("Transaction price must be greater than zero.");

        Price = price;
    }

    private void SetFee(decimal fee)
    {
        if (fee < 0)
            throw new DomainException("Fee must not be negative.");

        Fee = fee;
    }
}