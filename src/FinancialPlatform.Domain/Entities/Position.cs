namespace FinancialPlatform.Domain.Entities;

using FinancialPlatform.Domain.Exceptions;

public class Position : BaseEntity
{
    public Guid PortfolioId { get; private set; }
    public Guid AssetId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal? AverageEntryPrice { get; private set; }
    public decimal RealizedPnL { get; private set; }

    /// <summary>Zero quantity means the position is closed (data-model §4.8).</summary>
    public bool IsClosed => Quantity == 0;

    private Position() { }

    private Position(Guid portfolioId, Guid assetId)
    {
        SetPortfolioId(portfolioId);
        SetAssetId(assetId);
    }

    public static Position Create(Guid portfolioId, Guid assetId)
        => new(portfolioId, assetId);

    /// <summary>
    /// Increases the position using the weighted-average method (BR-021, BR-024):
    /// newAvg = (currentQty * avgEntry + qty * price) / (currentQty + qty).
    /// When the position is empty the average entry price equals the purchase price.
    /// </summary>
    public void ApplyBuy(decimal quantity, decimal price)
    {
        if (quantity <= 0)
            throw new DomainException("Buy quantity must be greater than zero.");

        if (price <= 0)
            throw new DomainException("Price must be greater than zero.");

        if (Quantity == 0)
        {
            AverageEntryPrice = price;
        }
        else
        {
            var previousQuantity = Quantity;
            var previousAverage = AverageEntryPrice ?? price;
            AverageEntryPrice = (previousQuantity * previousAverage + quantity * price) / (previousQuantity + quantity);
        }

        Quantity += quantity;
    }

    /// <summary>
    /// Decreases the position (BR-022). Long-only MVP: the sold quantity must not
    /// exceed the current position quantity (BR-023). Realized P/L accumulates
    /// (price - avgEntry) * quantity for each sell. When the quantity reaches
    /// zero the position is closed and the average entry price resets to null,
    /// consistent with "null until first buy" (BR-024).
    /// </summary>
    public void ApplySell(decimal quantity, decimal price)
    {
        if (quantity <= 0)
            throw new DomainException("Sell quantity must be greater than zero.");

        if (price <= 0)
            throw new DomainException("Price must be greater than zero.");

        if (quantity > Quantity)
            throw new DomainException("Cannot sell more than the current position quantity.");

        RealizedPnL += (price - (AverageEntryPrice ?? price)) * quantity;

        Quantity -= quantity;

        if (Quantity == 0)
            AverageEntryPrice = null;
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
}