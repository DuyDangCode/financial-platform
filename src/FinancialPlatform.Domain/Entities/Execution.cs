namespace FinancialPlatform.Domain.Entities;

using FinancialPlatform.Domain.Exceptions;

/// <summary>
/// Simulated execution result of an order. Immutable fact: created via
/// <see cref="Create"/> and never mutated. The "executed quantity ≤ order
/// quantity" check (BR-017) is enforced by the Order aggregate fill flow
/// (the execution entity holds no reference to its order).
/// </summary>
public class Execution : BaseEntity
{
    public Guid OrderId { get; private set; }
    public decimal ExecutedQuantity { get; private set; }
    public decimal ExecutionPrice { get; private set; }
    public decimal Fee { get; private set; }
    public DateTime ExecutedAt { get; private set; }

    private Execution() { }

    private Execution(
        Guid orderId,
        decimal executedQuantity,
        decimal executionPrice,
        decimal fee,
        DateTime executedAt)
    {
        SetOrderId(orderId);
        SetExecutedQuantity(executedQuantity);
        SetExecutionPrice(executionPrice);
        SetFee(fee);
        ExecutedAt = executedAt;
    }

    public static Execution Create(
        Guid orderId,
        decimal executedQuantity,
        decimal executionPrice,
        decimal fee = 0,
        DateTime? executedAt = null)
        => new(orderId, executedQuantity, executionPrice, fee, executedAt ?? DateTime.UtcNow);

    private void SetOrderId(Guid orderId)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("Order id is required.");

        OrderId = orderId;
    }

    private void SetExecutedQuantity(decimal executedQuantity)
    {
        if (executedQuantity <= 0)
            throw new DomainException("Executed quantity must be greater than zero.");

        ExecutedQuantity = executedQuantity;
    }

    private void SetExecutionPrice(decimal executionPrice)
    {
        if (executionPrice <= 0)
            throw new DomainException("Execution price must be greater than zero.");

        ExecutionPrice = executionPrice;
    }

    private void SetFee(decimal fee)
    {
        if (fee < 0)
            throw new DomainException("Fee must not be negative.");

        Fee = fee;
    }
}