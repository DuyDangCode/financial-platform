namespace FinancialPlatform.Domain.Entities;

using FinancialPlatform.Domain.Enums;
using FinancialPlatform.Domain.Exceptions;

public class Asset : BaseEntity
{
    public string Symbol { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public AssetType AssetType { get; private set; }
    public decimal CurrentPrice { get; private set; }
    public string Currency { get; private set; } = "USD";
    public DateTime PriceUpdatedAt { get; private set; }
    public AssetStatus Status { get; private set; } = AssetStatus.ACTIVE;

    private Asset() { }

    private Asset(string symbol, string name, AssetType assetType, decimal currentPrice, string currency)
    {
        SetSymbol(symbol);
        SetName(name);
        SetAssetType(assetType);
        SetCurrentPrice(currentPrice);
        SetCurrency(currency);
        PriceUpdatedAt = DateTime.UtcNow;
    }

    public static Asset Create(string symbol, string name, AssetType assetType, decimal currentPrice, string currency = "USD")
        => new(symbol, name, assetType, currentPrice, currency);

    public void RefreshPrice(decimal price)
    {
        SetCurrentPrice(price);
        PriceUpdatedAt = DateTime.UtcNow;
    }

    public void Activate() => Status = AssetStatus.ACTIVE;

    public void Deactivate() => Status = AssetStatus.INACTIVE;

    private void SetSymbol(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new DomainException("Asset symbol is required.");

        var normalized = symbol.Trim().ToUpperInvariant();

        if (normalized.Length > 16)
            throw new DomainException("Asset symbol must not exceed 16 characters.");

        Symbol = normalized;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Asset name is required.");

        if (name.Length > 256)
            throw new DomainException("Asset name must not exceed 256 characters.");

        Name = name.Trim();
    }

    private void SetAssetType(AssetType assetType)
    {
        if (!Enum.IsDefined(assetType))
            throw new DomainException("Asset type is invalid.");

        AssetType = assetType;
    }

    private void SetCurrentPrice(decimal currentPrice)
    {
        if (currentPrice < 0)
            throw new DomainException("Asset current price must not be negative.");

        CurrentPrice = currentPrice;
    }

    private void SetCurrency(string currency)
    {
        var normalized = string.IsNullOrWhiteSpace(currency)
            ? "USD"
            : currency.Trim().ToUpperInvariant();

        if (normalized.Length != 3)
            throw new DomainException("Currency must be a 3-letter ISO 4217 code.");

        Currency = normalized;
    }
}