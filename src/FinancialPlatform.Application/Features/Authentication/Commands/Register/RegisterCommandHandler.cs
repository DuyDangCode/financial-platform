namespace FinancialPlatform.Application.Features.Authentication.Commands.Register;

using FinancialPlatform.Application.Abstractions.Identity;
using FinancialPlatform.Application.Features.Authentication.DTOs;
using FinancialPlatform.Domain.Entities;
using FinancialPlatform.Domain.Exeptions;
using FinancialPlatform.Domain.Interface;

public class RegisterCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator
    )
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse> Handler(RegisterCommand command)
    {
        var passwordHash = _passwordHasher.Hash(command.Password);

        var userData = new User()
        {
            UserName = command.UserName,
            Email = command.UserName,
            PasswordHash = passwordHash,
            FirstName = command.FirstName,
            LastName = command.LastName,
            DisplayName = command.DisplayName,
            PhoneNumber = command.PhoneNumber,
        };

        var user = await User.Create(userData, _userRepository);

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResponse(
            token,
            expiresAt,
            user.Id,
            user.UserName,
            user.Email,
            user.DisplayName ?? user.UserName
        );
    }
}
