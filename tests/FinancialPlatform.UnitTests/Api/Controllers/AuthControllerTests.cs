namespace FinancialPlatform.UnitTests.Api.Controllers;

using System.Security.Claims;
using FinancialPlatform.Api.Controllers;
using FinancialPlatform.Api.Models.Request;
using FinancialPlatform.Api.Models.Response;
using FinancialPlatform.Application.Features.Authentication.Commands.ChangePassword;
using FinancialPlatform.Application.Features.Authentication.Commands.ForgotPassword;
using FinancialPlatform.Application.Features.Authentication.Commands.Login;
using FinancialPlatform.Application.Features.Authentication.Commands.Logout;
using FinancialPlatform.Application.Features.Authentication.Commands.Refresh;
using FinancialPlatform.Application.Features.Authentication.Commands.Register;
using FinancialPlatform.Application.Features.Authentication.Commands.ResetPassword;
using FinancialPlatform.Domain.Entities;
using FinancialPlatform.UnitTests.TestSupport;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Xunit;

public class AuthControllerTests
{
    private const string Email = "john.doe@example.com";
    private const string Password = "Sup3rSecret!";
    private const string UserName = "johndoe";

    [Fact]
    public async Task Register_ReturnsOkWithSuccessEnvelope_AndPersistsUser()
    {
        var harness = new Harness();

        var actionResult = await harness.Controller.Register(
            new RegisterRequest
            {
                UserName = "  john.doe  ",
                Email = "John.Doe@Example.com",
                Password = Password,
                FirstName = "John",
                LastName = "Doe",
            },
            CancellationToken.None);

        var response = ResponseAssert.OkValue(actionResult);
        Assert.True(response.Success);
        Assert.Equal("Registration successful.", response.Message);

        var persistedUser = Assert.Single(harness.Users.Users);
        Assert.Equal("john.doe", persistedUser.UserName);
        Assert.Equal(Email, persistedUser.Email);
        Assert.Equal(harness.Hasher.Hash(Password), persistedUser.PasswordHash);
        Assert.Equal("John", persistedUser.FirstName);
        Assert.Equal("Doe", persistedUser.LastName);
        Assert.Same(persistedUser, Assert.Single(harness.Sessions.IssuedFor));
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithSuccessEnvelope()
    {
        var harness = new Harness();
        var user = harness.SeedActiveUser(Email, Password);

        var actionResult = await harness.Controller.Login(
            new LoginRequest { Email = Email, Password = Password },
            CancellationToken.None);

        var response = ResponseAssert.OkValue(actionResult);
        Assert.True(response.Success);
        Assert.Equal("Login successful.", response.Message);
        Assert.Equal(user.Id, response.Data!.UserId);
        Assert.Equal(FakeAuthSessionIssuer.AccessToken, response.Data.Token);
        Assert.Equal(FakeAuthSessionIssuer.ExpiresAtUtc, response.Data.ExpiresAt);
        Assert.Equal(FakeAuthSessionIssuer.RefreshTokenValue, response.Data.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewSession_AndRevokesOldToken()
    {
        var harness = new Harness();
        var user = harness.SeedActiveUser(Email, Password);
        var refreshToken = RefreshToken.Create(
            user.Id,
            "valid-refresh-token",
            DateTime.UtcNow.AddMinutes(30));
        harness.RefreshTokens.Seed(refreshToken);

        var actionResult = await harness.Controller.Refresh(
            new RefreshRequest { RefreshToken = "valid-refresh-token" },
            CancellationToken.None);

        var response = ResponseAssert.OkValue(actionResult);
        Assert.True(response.Success);
        Assert.Equal("Token refreshed successfully.", response.Message);
        Assert.Equal(user.Id, response.Data!.UserId);
        Assert.False(refreshToken.IsActive);
    }

    [Fact]
    public async Task Logout_ReturnsOkWithHandlerMessage_AndRevokesActiveToken()
    {
        var harness = new Harness();
        var user = harness.SeedActiveUser(Email, Password);
        var refreshToken = RefreshToken.Create(
            user.Id,
            "active-refresh-token",
            DateTime.UtcNow.AddMinutes(30));
        harness.RefreshTokens.Seed(refreshToken);

        var actionResult = await harness.Controller.Logout(
            new LogoutRequest { RefreshToken = "active-refresh-token" },
            CancellationToken.None);

        var response = ResponseAssert.OkValue(actionResult);
        Assert.True(response.Success);
        Assert.Equal("Logged out successfully.", response.Message);
        Assert.False(refreshToken.IsActive);
    }

    [Fact]
    public async Task ChangePassword_Unauthenticated_ReturnsUnauthorized()
    {
        var harness = new Harness();
        harness.AuthenticateUnauthenticated();

        var actionResult = await harness.Controller.ChangePassword(
            new ChangePasswordRequest { CurrentPassword = Password, NewPassword = NewPassword },
            CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(actionResult.Result);
    }

    [Fact]
    public async Task ChangePassword_NonGuidSubjectClaim_ReturnsUnauthorized()
    {
        var harness = new Harness();
        harness.Authenticate(ClaimTypes.NameIdentifier, "not-a-guid");

        var actionResult = await harness.Controller.ChangePassword(
            new ChangePasswordRequest { CurrentPassword = Password, NewPassword = NewPassword },
            CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(actionResult.Result);
    }

    [Fact]
    public async Task ChangePassword_EmptyGuidClaim_ReturnsUnauthorized()
    {
        var harness = new Harness();
        harness.Authenticate(ClaimTypes.NameIdentifier, Guid.Empty.ToString());

        var actionResult = await harness.Controller.ChangePassword(
            new ChangePasswordRequest { CurrentPassword = Password, NewPassword = NewPassword },
            CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(actionResult.Result);
    }

    [Fact]
    public async Task ChangePassword_WithNameIdentifierClaim_ChangesPassword()
    {
        var harness = new Harness();
        var user = harness.SeedActiveUser(Email, Password);
        harness.Authenticate(ClaimTypes.NameIdentifier, user.Id.ToString());

        var actionResult = await harness.Controller.ChangePassword(
            new ChangePasswordRequest { CurrentPassword = Password, NewPassword = NewPassword },
            CancellationToken.None);

        var response = ResponseAssert.OkValue(actionResult);
        Assert.True(response.Success);
        Assert.Equal("Password changed successfully.", response.Message);
        Assert.Equal(harness.Hasher.Hash(NewPassword), user.PasswordHash);
    }

    [Fact]
    public async Task ChangePassword_FallsBackToSubClaim_ChangesPasswordAndRevokesSessions()
    {
        var harness = new Harness();
        var user = harness.SeedActiveUser(Email, Password);
        var refreshToken = RefreshToken.Create(
            user.Id,
            "stale-session-token",
            DateTime.UtcNow.AddMinutes(30));
        harness.RefreshTokens.Seed(refreshToken);
        harness.Authenticate("sub", user.Id.ToString());

        var actionResult = await harness.Controller.ChangePassword(
            new ChangePasswordRequest { CurrentPassword = Password, NewPassword = NewPassword },
            CancellationToken.None);

        var response = ResponseAssert.OkValue(actionResult);
        Assert.True(response.Success);
        Assert.Equal(harness.Hasher.Hash(NewPassword), user.PasswordHash);
        Assert.False(refreshToken.IsActive);
    }

    [Fact]
    public async Task ForgotPassword_DevelopmentEnvironment_ExposesResetCode()
    {
        var harness = new Harness();
        harness.Environment.EnvironmentName = Environments.Development;
        harness.SeedActiveUser(Email, Password);

        var actionResult = await harness.Controller.ForgotPassword(
            new ForgotPasswordRequest { Email = Email },
            CancellationToken.None);

        var response = ResponseAssert.OkValue(actionResult);
        Assert.True(response.Success);
        Assert.True(response.Data!.Delivered);
        Assert.Matches("^\\d{6}$", response.Data!.ResetCode);
        Assert.Single(harness.PasswordResetTokens.Tokens);
    }

    [Fact]
    public async Task ForgotPassword_ProductionEnvironment_HidesResetCode()
    {
        var harness = new Harness();
        harness.SeedActiveUser(Email, Password);

        var actionResult = await harness.Controller.ForgotPassword(
            new ForgotPasswordRequest { Email = Email },
            CancellationToken.None);

        var response = ResponseAssert.OkValue(actionResult);
        Assert.True(response.Data!.Delivered);
        Assert.Null(response.Data!.ResetCode);
        Assert.Single(harness.PasswordResetTokens.Tokens);
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_ReturnsNotDelivered_WithoutCreatingToken()
    {
        var harness = new Harness();

        var actionResult = await harness.Controller.ForgotPassword(
            new ForgotPasswordRequest { Email = "ghost@example.com" },
            CancellationToken.None);

        var response = ResponseAssert.OkValue(actionResult);
        Assert.True(response.Success);
        Assert.False(response.Data!.Delivered);
        Assert.Null(response.Data!.ResetCode);
        Assert.Empty(harness.PasswordResetTokens.Tokens);
    }

    [Fact]
    public async Task ResetPassword_WithValidCode_ResetsPassword()
    {
        var harness = new Harness();
        var user = harness.SeedActiveUser(Email, Password);
        await harness.PasswordResetTokens.AddAsync(
            PasswordResetToken.Create(user.Id, "123456", DateTime.UtcNow.AddMinutes(15)));

        var actionResult = await harness.Controller.ResetPassword(
            new ResetPasswordRequest { Email = Email, Code = "123456", NewPassword = NewPassword },
            CancellationToken.None);

        var response = ResponseAssert.OkValue(actionResult);
        Assert.True(response.Success);
        Assert.Equal("Password has been reset successfully.", response.Message);
        Assert.Equal(harness.Hasher.Hash(NewPassword), user.PasswordHash);
    }

    private const string NewPassword = "N3wSecret!";

    private static class ResponseAssert
    {
        public static ApiResponse<T> OkValue<T>(ActionResult<ApiResponse<T>> actionResult)
        {
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            return Assert.IsAssignableFrom<ApiResponse<T>>(okResult.Value);
        }
    }

    private sealed class Harness
    {
        public FakeUserRepository Users { get; } = new();

        public FakeRefreshTokenRepository RefreshTokens { get; } = new();

        public FakePasswordResetTokenRepository PasswordResetTokens { get; } = new();

        public StubPasswordHasher Hasher { get; } = new();

        public FakeAuthSessionIssuer Sessions { get; } = new();

        public FakeWebHostEnvironment Environment { get; } = new();

        public AuthController Controller { get; }

        public Harness()
        {
            Controller = new AuthController(
                new LoginCommandHandler(Users, Hasher, Sessions),
                new RegisterCommandHandler(Users, Hasher, Sessions),
                new RefreshCommandHandler(RefreshTokens, Users, Sessions),
                new LogoutCommandHandler(RefreshTokens),
                new ChangePasswordCommandHandler(Users, RefreshTokens, Hasher),
                new ForgotPasswordCommandHandler(Users, PasswordResetTokens),
                new ResetPasswordCommandHandler(Users, PasswordResetTokens, RefreshTokens, Hasher),
                Environment);
        }

        public User SeedActiveUser(string email, string password)
        {
            var user = User.Create(UserName, email, Hasher.Hash(password), "John", "Doe");
            Users.Seed(user);
            return user;
        }

        public void AuthenticateUnauthenticated()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            Controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        public void Authenticate(string claimType, string claimValue)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(claimType, claimValue) }, "TestAuth"));
            Controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }
    }
}
