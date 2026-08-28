using FinancialPlatform.Application.Abstractions.Identity;
using FinancialPlatform.Application.Features.Authentication.DTOs;
using FinancialPlatform.Domain.Exceptions;
using FinancialPlatform.Domain.Interfaces;

namespace FinancialPlatform.Application.Features.Authentication.Commands.Login;

public sealed class LoginCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthSessionIssuer _authSessionIssuer;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAuthSessionIssuer authSessionIssuer)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _authSessionIssuer = authSessionIssuer;
    }

    public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken)
            ?? throw new InvalidCredentialsException();

        if (!user.IsActive)
        {
            throw new InvalidCredentialsException();
        }

        if (!_passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        return await _authSessionIssuer.IssueAsync(user, cancellationToken);
    }
}
