using FinancialPlatform.Application.Abstractions.Identity;
using FinancialPlatform.Application.Features.Authentication.DTOs;
using FinancialPlatform.Domain.Exceptions;
using FinancialPlatform.Domain.Interfaces;

namespace FinancialPlatform.Application.Features.Authentication.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<MessageResponse> Handle(ResetPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var resetToken = await _passwordResetTokenRepository.GetActiveByCodeAsync(command.Code, cancellationToken);

        if (resetToken is null || !resetToken.IsActive)
        {
            throw new InvalidOrExpiredTokenException();
        }

        var user = await _userRepository.GetByIdAsync(resetToken.UserId, cancellationToken)
            ?? throw new InvalidOrExpiredTokenException();

        if (!string.Equals(user.Email, command.Email, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOrExpiredTokenException();
        }

        user.SetPassword(_passwordHasher.Hash(command.NewPassword));
        await _userRepository.UpdateAsync(user, cancellationToken);

        resetToken.MarkUsed();
        await _passwordResetTokenRepository.InvalidateAllForUserAsync(user.Id, cancellationToken);

        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, cancellationToken);

        return new MessageResponse("Password has been reset successfully.");
    }
}
