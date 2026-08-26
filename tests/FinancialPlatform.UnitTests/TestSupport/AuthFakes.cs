namespace FinancialPlatform.UnitTests.TestSupport;

using FinancialPlatform.Application.Abstractions.Identity;
using FinancialPlatform.Application.Features.Authentication.DTOs;
using FinancialPlatform.Domain.Entities;
using FinancialPlatform.Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

public sealed class StubPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => $"hashed::{password}";

    public bool Verify(string password, string hash) => hash == Hash(password);
}

public sealed class FakeAuthSessionIssuer : IAuthSessionIssuer
{
    public const string AccessToken = "test-access-token";
    public const string RefreshTokenValue = "test-refresh-token";
    public static readonly DateTime ExpiresAtUtc = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public List<User> IssuedFor { get; } = new();

    public Task<LoginResponse> IssueAsync(User user, CancellationToken cancellationToken = default)
    {
        IssuedFor.Add(user);

        return Task.FromResult(new LoginResponse(
            AccessToken,
            ExpiresAtUtc,
            user.Id,
            user.UserName,
            user.Email,
            user.DisplayName ?? string.Empty,
            RefreshTokenValue));
    }
}

public sealed class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _usersById = new();

    public void Seed(User user) => _usersById[user.Id] = user;

    public IReadOnlyCollection<User> Users => _usersById.Values;

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => Task.FromResult(_usersById.Values.FirstOrDefault(u => u.Email == email.ToLowerInvariant()));

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_usersById.GetValueOrDefault(id));

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _usersById.Add(user.Id, user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _usersById[user.Id] = user;
        return Task.CompletedTask;
    }
}

public sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly List<RefreshToken> _tokens = new();

    public void Seed(RefreshToken token) => _tokens.Add(token);

    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        => Task.FromResult(_tokens.FirstOrDefault(t => t.Token == token));

    public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _tokens.Add(refreshToken);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        foreach (var token in _tokens.Where(t => t.UserId == userId))
        {
            token.Revoke();
        }

        return Task.CompletedTask;
    }
}

public sealed class FakePasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly List<PasswordResetToken> _tokens = new();

    public IReadOnlyCollection<PasswordResetToken> Tokens => _tokens;

    public Task<PasswordResetToken?> GetActiveByCodeAsync(string code, CancellationToken cancellationToken = default)
        => Task.FromResult(_tokens.FirstOrDefault(t => t.Code == code && t.IsActive));

    public Task AddAsync(PasswordResetToken passwordResetToken, CancellationToken cancellationToken = default)
    {
        _tokens.Add(passwordResetToken);
        return Task.CompletedTask;
    }

    public Task InvalidateAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        foreach (var token in _tokens.Where(t => t.UserId == userId))
        {
            token.MarkUsed();
        }

        return Task.CompletedTask;
    }
}

public sealed class FakeWebHostEnvironment : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "FinancialPlatform.Api";

    public string EnvironmentName { get; set; } = Environments.Production;

    public string ContentRootPath { get; set; } = "/";

    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

    public string WebRootPath { get; set; } = "/";

    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
}
