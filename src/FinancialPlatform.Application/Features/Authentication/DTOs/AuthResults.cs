namespace FinancialPlatform.Application.Features.Authentication.DTOs;

public sealed record MessageResponse(string Message);

public sealed record ForgotPasswordResult(bool Success, string? ResetCode);
