using System.Net;
using System.Text.Json;
using Bruno.Application.Common.Models;
using Bruno.Domain.Common;
using FluentValidation;

namespace Bruno.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (status, payload) = exception switch
        {
            ValidationException validation => (
                HttpStatusCode.BadRequest,
                new ApiError
                {
                    Message = "Validation failed.",
                    Errors = validation.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
                }),
            DomainException domain => (
                HttpStatusCode.BadRequest,
                new ApiError { Message = domain.Message }),
            _ => (
                HttpStatusCode.InternalServerError,
                new ApiError { Message = "An unexpected error occurred." })
        };

        if (status == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {Message}", payload.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
