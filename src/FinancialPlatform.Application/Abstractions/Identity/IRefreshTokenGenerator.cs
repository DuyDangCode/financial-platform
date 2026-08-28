namespace FinancialPlatform.Application.Abstractions.Identity;

public interface IRefreshTokenGenerator
{
    (string Token, DateTime ExpiresAt) Generate();
}
