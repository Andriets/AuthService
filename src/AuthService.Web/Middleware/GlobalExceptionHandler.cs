using AuthService.Web.Core.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Web.Middleware;

internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occured");

        return exception switch
        {
            AppException appEx => await HandleAppExceptionAsync(httpContext, appEx, cancellationToken),
            ValidationException validationEx => await HandleValidationExceptionAsync(httpContext, validationEx, cancellationToken),
            _ => await HandleUnknownExceptionAsync(httpContext, exception, cancellationToken)
        };
    }

    private async ValueTask<bool> HandleAppExceptionAsync(
        HttpContext httpContext,
        AppException exception,
        CancellationToken _)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = "Bad Request",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            }
        });
    }

    private async ValueTask<bool> HandleValidationExceptionAsync(
        HttpContext httpContext,
        ValidationException exception,
        CancellationToken _)
    {
        httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new HttpValidationProblemDetails(errors)
            {
                Title = "Validation Failed",
                Status = StatusCodes.Status422UnprocessableEntity
            }
        });
    }

    private async ValueTask<bool> HandleUnknownExceptionAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken _)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError
            }
        });
    }
}