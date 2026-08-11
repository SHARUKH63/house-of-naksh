using HouseOfNaksh.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HouseOfNaksh.Api.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, "Business rule violated"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception on {Path}", context.Request.Path);
        else
            logger.LogWarning("{ExceptionType} on {Path}: {Message}",
                exception.GetType().Name, context.Request.Path, exception.Message);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred. Please contact support with the trace id."
                : exception.Message,
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = System.Diagnostics.Activity.Current?.TraceId.ToString();

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, ct);
        return true;
    }
}
