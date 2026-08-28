using System.Security.Cryptography;
using FinancialPlatform.Application.Features.Authentication.DTOs;
using FinancialPlatform.Domain.Entities;
using FinancialPlatform.Domain.Interfaces;

namespace FinancialPlatform.Application.Features.Authentication.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler
{
    private const int CodeLength = 6;
    private const int LifetimeMinutes = 15;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository)
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
    }

    public async Task<ForgotPasswordResult> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return new ForgotPasswordResult(false, null);
        }

        await _passwordResetTokenRepository.InvalidateAllForUserAsync(user.Id, cancellationToken);

        var code = RandomNumberGenerator.GetInt32(0, (int)Math.Pow(10, CodeLength)).ToString($"D{CodeLength}");
        await _passwordResetTokenRepository.AddAsync(
            PasswordResetToken.Create(user.Id, code, DateTime.UtcNow.AddMinutes(LifetimeMinutes)),
            cancellationToken);

        return new ForgotPasswordResult(true, code);
    }
}
