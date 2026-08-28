using FinancialPlatform.Application.Abstractions.Identity;
using FinancialPlatform.Application.Features.Authentication.DTOs;
using FinancialPlatform.Domain.Entities;
using FinancialPlatform.Domain.Interfaces;

namespace FinancialPlatform.Infrastructure.Identity;

public class AuthSessionIssuer : IAuthSessionIssuer
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthSessionIssuer(
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<LoginResponse> IssueAsync(User user, CancellationToken cancellationToken = default)
    {
        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        var (refreshToken, refreshTokenExpiresAt) = _refreshTokenGenerator.Generate();
        await _refreshTokenRepository.AddAsync(
            RefreshToken.Create(user.Id, refreshToken, refreshTokenExpiresAt),
            cancellationToken);

        return new LoginResponse(
            token,
            expiresAt,
            user.Id,
            user.UserName,
            user.Email,
            user.DisplayName ?? user.UserName,
            refreshToken);
    }
}
