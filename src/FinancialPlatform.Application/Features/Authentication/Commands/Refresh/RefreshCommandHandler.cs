using FinancialPlatform.Application.Abstractions.Identity;
using FinancialPlatform.Application.Features.Authentication.DTOs;
using FinancialPlatform.Domain.Exceptions;
using FinancialPlatform.Domain.Interfaces;

namespace FinancialPlatform.Application.Features.Authentication.Commands.Refresh;

public sealed class RefreshCommandHandler
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuthSessionIssuer _authSessionIssuer;

    public RefreshCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IAuthSessionIssuer authSessionIssuer)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _authSessionIssuer = authSessionIssuer;
    }

    public async Task<LoginResponse> Handle(RefreshCommand command, CancellationToken cancellationToken = default)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(command.RefreshToken, cancellationToken)
            ?? throw new InvalidOrExpiredTokenException();

        if (!storedToken.IsActive)
        {
            throw new InvalidOrExpiredTokenException();
        }

        var user = await _userRepository.GetByIdAsync(storedToken.UserId, cancellationToken)
            ?? throw new InvalidCredentialsException();

        if (!user.IsActive)
        {
            throw new InvalidCredentialsException();
        }

        storedToken.Revoke();
        await _refreshTokenRepository.UpdateAsync(storedToken, cancellationToken);

        return await _authSessionIssuer.IssueAsync(user, cancellationToken);
    }
}
