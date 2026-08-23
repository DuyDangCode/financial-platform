using FinancialPlatform.Application.Abstractions.Identity;
using FinancialPlatform.Application.Features.Authentication.DTOs;
using FinancialPlatform.Domain.Exeptions;
using FinancialPlatform.Domain.Interface;

namespace FinancialPlatform.Application.Features.Authentication.Commands.Login;

public sealed class LoginCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
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

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResponse(
            token,
            expiresAt,
            user.Id,
            user.UserName,
            user.Email,
            user.DisplayName ?? user.UserName);
    }
}