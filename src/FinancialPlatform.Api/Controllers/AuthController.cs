using System.Security.Claims;
using FinancialPlatform.Api.Models.Request;
using FinancialPlatform.Api.Models.Response;
using FinancialPlatform.Application.Features.Authentication.Commands.ChangePassword;
using FinancialPlatform.Application.Features.Authentication.Commands.ForgotPassword;
using FinancialPlatform.Application.Features.Authentication.Commands.Login;
using FinancialPlatform.Application.Features.Authentication.Commands.Logout;
using FinancialPlatform.Application.Features.Authentication.Commands.Refresh;
using FinancialPlatform.Application.Features.Authentication.Commands.Register;
using FinancialPlatform.Application.Features.Authentication.Commands.ResetPassword;
using FinancialPlatform.Application.Features.Authentication.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LoginCommandHandler _loginCommandHandler;
    private readonly RegisterCommandHandler _registerCommandHandler;
    private readonly RefreshCommandHandler _refreshCommandHandler;
    private readonly LogoutCommandHandler _logoutCommandHandler;
    private readonly ChangePasswordCommandHandler _changePasswordCommandHandler;
    private readonly ForgotPasswordCommandHandler _forgotPasswordCommandHandler;
    private readonly ResetPasswordCommandHandler _resetPasswordCommandHandler;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        LoginCommandHandler loginCommandHandler,
        RegisterCommandHandler registerCommandHandler,
        RefreshCommandHandler refreshCommandHandler,
        LogoutCommandHandler logoutCommandHandler,
        ChangePasswordCommandHandler changePasswordCommandHandler,
        ForgotPasswordCommandHandler forgotPasswordCommandHandler,
        ResetPasswordCommandHandler resetPasswordCommandHandler,
        IWebHostEnvironment environment
    )
    {
        _loginCommandHandler = loginCommandHandler;
        _registerCommandHandler = registerCommandHandler;
        _refreshCommandHandler = refreshCommandHandler;
        _logoutCommandHandler = logoutCommandHandler;
        _changePasswordCommandHandler = changePasswordCommandHandler;
        _forgotPasswordCommandHandler = forgotPasswordCommandHandler;
        _resetPasswordCommandHandler = resetPasswordCommandHandler;
        _environment = environment;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.UserName,
            request.FirstName,
            request.LastName,
            request.DisplayName,
            request.PhoneNumber
        );

        var result = await _registerCommandHandler.Handle(command, cancellationToken);

        return Ok(ApiResponse<LoginResponse>.SuccessResponse(result, "Registration successful."));
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

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new RefreshCommand(request.RefreshToken);
        var result = await _refreshCommandHandler.Handle(command, cancellationToken);

        return Ok(
            ApiResponse<LoginResponse>.SuccessResponse(result, "Token refreshed successfully.")
        );
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<MessageResponse>>> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new LogoutCommand(request.RefreshToken);
        var result = await _logoutCommandHandler.Handle(command, cancellationToken);

        return Ok(ApiResponse<MessageResponse>.SuccessResponse(result, result.Message));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<MessageResponse>>> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new ChangePasswordCommand(
            userId,
            request.CurrentPassword,
            request.NewPassword
        );
        var result = await _changePasswordCommandHandler.Handle(command, cancellationToken);

        return Ok(ApiResponse<MessageResponse>.SuccessResponse(result, result.Message));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<ForgotPasswordResponse>>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new ForgotPasswordCommand(request.Email);
        var result = await _forgotPasswordCommandHandler.Handle(command, cancellationToken);

        // Reset code is only exposed in Development until an email service exists.
        var response = new ForgotPasswordResponse(
            result.Success,
            _environment.IsDevelopment() ? result.ResetCode : null
        );

        return Ok(
            ApiResponse<ForgotPasswordResponse>.SuccessResponse(
                response,
                "If the email address is registered, a reset code has been sent."
            )
        );
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<MessageResponse>>> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new ResetPasswordCommand(request.Email, request.Code, request.NewPassword);
        var result = await _resetPasswordCommandHandler.Handle(command, cancellationToken);

        return Ok(ApiResponse<MessageResponse>.SuccessResponse(result, result.Message));
    }

    private bool TryGetUserId(out Guid userId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out userId) && userId != Guid.Empty;
    }
}
