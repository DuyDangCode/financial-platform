namespace FinancialPlatform.Domain.Entities;

using FinancialPlatform.Domain.Exceptions;

public class PasswordResetToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    public bool IsActive => UsedAt is null && DateTime.UtcNow < ExpiresAt;

    private PasswordResetToken() { }

    private PasswordResetToken(Guid userId, string code, DateTime expiresAt)
    {
        SetUserId(userId);
        SetCode(code);

        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("Expiration date must be in the future.");

        ExpiresAt = expiresAt;
    }

    public static PasswordResetToken Create(Guid userId, string code, DateTime expiresAt)
        => new(userId, code, expiresAt);

    public void MarkUsed() => UsedAt ??= DateTime.UtcNow;

    private void SetUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User id is required.");

        UserId = userId;
    }

    private void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Code is required.");

        Code = code;
    }
}
