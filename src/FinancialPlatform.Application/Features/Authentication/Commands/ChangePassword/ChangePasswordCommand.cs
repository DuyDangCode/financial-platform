namespace FinancialPlatform.Application.Features.Authentication.Commands.ChangePassword;

public sealed record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword);
