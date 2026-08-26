using System.Net;
using System.Text.Json;
using FinancialPlatform.Api.Models.Response;
using FinancialPlatform.Domain.Exceptions;

namespace FinancialPlatform.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = exception switch
        {
            UserAlreadyExistsException => HttpStatusCode.Conflict,
            InvalidCredentialsException => HttpStatusCode.Unauthorized,
            DomainException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = (int)statusCode;

        var message = statusCode == HttpStatusCode.InternalServerError
            ? "An unexpected error occurred. Please try again later."
            : exception.Message;

        var errorResponse = new ErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}