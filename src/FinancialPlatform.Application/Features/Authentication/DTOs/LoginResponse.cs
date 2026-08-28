namespace FinancialPlatform.Application.Features.Authentication.DTOs;

public sealed record LoginResponse(
    string Token,
    DateTime ExpiresAt,
    Guid UserId,
    string UserName,
    string Email,
    string DisplayName,
    string? RefreshToken);