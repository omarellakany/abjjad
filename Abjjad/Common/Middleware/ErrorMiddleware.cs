using System.Net;
using System.Text.Json;
using Abjjad.Common.Models;
using FluentValidation;

namespace Abjjad.Common.Middleware;

public class ErrorMiddleware(RequestDelegate next, ILogger<ErrorMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            ValidationException validationEx => new ErrorResponse(
                "Validation failed",
                validationEx.Errors.Select(e => e.ErrorMessage).ToArray(),
                (int)HttpStatusCode.BadRequest),

            FileNotFoundException => new ErrorResponse(
                "Resource not found",
                [exception.Message],
                (int)HttpStatusCode.NotFound),

            ArgumentException => new ErrorResponse(
                "Invalid argument",
                [exception.Message],
                (int)HttpStatusCode.BadRequest),

            _ => new ErrorResponse(
                "An error occurred while processing your request",
                [exception.Message],
                (int)HttpStatusCode.InternalServerError)
        };

        logger.LogError(exception, "An error occurred: {Message}", exception.Message);
        response.StatusCode = errorResponse.StatusCode;

        var result = JsonSerializer.Serialize(errorResponse);
        await response.WriteAsync(result);
    }
}