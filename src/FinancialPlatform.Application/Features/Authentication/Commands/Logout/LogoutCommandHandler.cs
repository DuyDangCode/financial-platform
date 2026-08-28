using FinancialPlatform.Application.Features.Authentication.DTOs;
using FinancialPlatform.Domain.Interfaces;

namespace FinancialPlatform.Application.Features.Authentication.Commands.Logout;

public sealed class LogoutCommandHandler
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<MessageResponse> Handle(LogoutCommand command, CancellationToken cancellationToken = default)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(command.RefreshToken, cancellationToken);

        if (storedToken is { IsActive: true })
        {
            storedToken.Revoke();
            await _refreshTokenRepository.UpdateAsync(storedToken, cancellationToken);
        }

        return new MessageResponse("Logged out successfully.");
    }
}
