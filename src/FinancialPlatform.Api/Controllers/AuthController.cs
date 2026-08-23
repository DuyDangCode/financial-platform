using FinancialPlatform.Api.Models.Request;
using FinancialPlatform.Api.Models.Response;
using FinancialPlatform.Application.Features.Authentication.Commands.Login;
using FinancialPlatform.Application.Features.Authentication.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LoginCommandHandler _loginCommandHandler;

    public AuthController(LoginCommandHandler loginCommandHandler)
    {
        _loginCommandHandler = loginCommandHandler;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _loginCommandHandler.Handle(command, cancellationToken);

        return Ok(ApiResponse<LoginResponse>.SuccessResponse(result, "Login successful."));
    }

    [HttpPost("login")]
    public async void Register(RegisterRequest request) { }

    [HttpPost("refresh")]
    public async void Refresh(string refreshToken) { }

    [HttpPost("logout")]
    public async void Logout(string accessToken) { }

    [HttpPost("change-password")]
    public async void ChangePassword(string accessToken) { }

    [HttpPost("forgot-password")]
    public async void ForgotPassword(string accessToken) { }

    [HttpPost("reset-password")]
    public async void ResetPassword(string accessToken) { }
}

