using FinancialPlatform.Application.Abstractions.Identity;
using FinancialPlatform.Application.Features.Authentication.DTOs;
using FinancialPlatform.Domain.Exceptions;
using FinancialPlatform.Domain.Interfaces;

namespace FinancialPlatform.Application.Features.Authentication.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<MessageResponse> Handle(ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new InvalidCredentialsException();

        if (!user.IsActive || !_passwordHasher.Verify(command.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        user.SetPassword(_passwordHasher.Hash(command.NewPassword));
        await _userRepository.UpdateAsync(user, cancellationToken);

        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, cancellationToken);

        return new MessageResponse("Password changed successfully.");
    }
}
