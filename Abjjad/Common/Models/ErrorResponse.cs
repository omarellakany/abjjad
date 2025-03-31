namespace Abjjad.Common.Models;

public class ErrorResponse(string message, string[] errors, int statusCode)
{
    public string Message { get; } = message;
    public string[] Errors { get; } = errors;
    public int StatusCode { get; } = statusCode;
}