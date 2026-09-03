namespace FinancialPlatform.Domain.Entities;

using FinancialPlatform.Domain.Enums;
using FinancialPlatform.Domain.Exceptions;

public class Portfolio : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string BaseCurrency { get; private set; } = "USD";
    public PortfolioStatus Status { get; private set; } = PortfolioStatus.ACTIVE;
    public string? Notes { get; private set; }

    private Portfolio() { }

    private Portfolio(Guid userId, string name, string baseCurrency, string? notes)
    {
        SetUserId(userId);
        SetName(name);
        SetBaseCurrency(baseCurrency);
        SetNotes(notes);
    }

    public static Portfolio Create(Guid userId, string name, string baseCurrency = "USD", string? notes = null)
        => new(userId, name, baseCurrency, notes);

    public void Rename(string name) => SetName(name);

    public void Activate() => Status = PortfolioStatus.ACTIVE;

    public void Disable() => Status = PortfolioStatus.DISABLED;

    private void SetUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User id is required.");

        UserId = userId;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Portfolio name is required.");

        if (name.Length > 128)
            throw new DomainException("Portfolio name must not exceed 128 characters.");

        Name = name.Trim();
    }

    private void SetBaseCurrency(string baseCurrency)
    {
        var currency = string.IsNullOrWhiteSpace(baseCurrency)
            ? "USD"
            : baseCurrency.Trim().ToUpperInvariant();

        if (currency.Length != 3)
            throw new DomainException("Currency must be a 3-letter ISO 4217 code.");

        BaseCurrency = currency;
    }

    private void SetNotes(string? notes)
    {
        if (notes is not null && notes.Length > 512)
            throw new DomainException("Notes must not exceed 512 characters.");

        Notes = notes;
    }
}