namespace FinancialPlatform.Domain.Entities;

using FinancialPlatform.Domain.Exceptions;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;

    private RefreshToken() { }

    private RefreshToken(Guid userId, string token, DateTime expiresAt)
    {
        SetUserId(userId);
        SetToken(token);

        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("Expiration date must be in the future.");

        ExpiresAt = expiresAt;
    }

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt)
        => new(userId, token, expiresAt);

    public void Revoke() => RevokedAt ??= DateTime.UtcNow;

    private void SetUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User id is required.");

        UserId = userId;
    }

    private void SetToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new DomainException("Token is required.");

        Token = token;
    }
}
