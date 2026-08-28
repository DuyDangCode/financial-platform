using System.Security.Cryptography;
using FinancialPlatform.Application.Abstractions.Identity;

namespace FinancialPlatform.Infrastructure.Identity;

public class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private const int TokenBytes = 64;
    private const int LifetimeDays = 7;

    public (string Token, DateTime ExpiresAt) Generate()
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(TokenBytes));
        return (token, DateTime.UtcNow.AddDays(LifetimeDays));
    }
}
