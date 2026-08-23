using FinancialPlatform.Domain.Entities;

namespace FinancialPlatform.Application.Abstractions.Identity;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}