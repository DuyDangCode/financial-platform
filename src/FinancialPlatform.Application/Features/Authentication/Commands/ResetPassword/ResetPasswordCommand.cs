namespace FinancialPlatform.Application.Features.Authentication.Commands.ResetPassword;

public sealed record ResetPasswordCommand(string Email, string Code, string NewPassword);
