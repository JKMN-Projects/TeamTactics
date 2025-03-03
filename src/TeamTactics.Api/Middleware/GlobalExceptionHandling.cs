﻿using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using TeamTactics.Application.Common.Exceptions;

namespace TeamTactics.Api.Middleware
{
    /// <summary>
    /// Global exception handling middleware that catches 
    /// unhandled exceptions and returns a ProblemDetails response.
    /// </summary>
    public class GlobalExceptionHandling : IExceptionHandler
    {
        private readonly ProblemDetailsFactory _problemDetailsFactory;
        private readonly ILogger<GlobalExceptionHandling> _logger;

        public GlobalExceptionHandling(ILogger<GlobalExceptionHandling> logger, ProblemDetailsFactory problemDetailsFactory)
        {
            _logger = logger;
            _problemDetailsFactory = problemDetailsFactory;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred");

            bool isUserAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false;

            int statusCode = StatusCodes.Status500InternalServerError;
            string errorMessage = "An error occurred while processing your request.";
            Dictionary<string, List<string>> validationErrors = [];

            // Check if the exception is a known type and shaping response to reflect that
            switch (exception)
            {
                case ArgumentException argEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    errorMessage = exception.Message;
                    if (argEx.ParamName is not null)
                    {
                        validationErrors.Add(argEx.ParamName, [argEx.Message]);
                    }
                    break;
                case ValidationException validationEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    errorMessage = "Validation error occurred.";
                    validationErrors = validationEx.Errors.ToDictionary();
                    break;
                case UnauthorizedException unauthEx:
                    statusCode = StatusCodes.Status401Unauthorized;
                    errorMessage = "Unauthorized.";
                    break;
                case EntityNotFoundException entityNotFoundEx:
                    statusCode = StatusCodes.Status404NotFound;
                    errorMessage = "Entity not found.";
                    break;
            }


            // Build the response object
            ProblemDetails problemDetails = _problemDetailsFactory.CreateProblemDetails(
                httpContext, 
                statusCode, 
                title: errorMessage);

            if (validationErrors.Count != 0)
            {
                problemDetails.Extensions.Add("validationErrors", validationErrors);
            }

            // Override the response
            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}
