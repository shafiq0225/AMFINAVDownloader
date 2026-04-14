using System.Net;
using System.Text.Json;
using AMFINAV.SchemeAPI.Application.DTOs;
using AMFINAV.SchemeAPI.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace AMFINAV.SchemeAPI.API.Middleware
{
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
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(
            HttpContext context, Exception exception)
        {
            var traceId = context.TraceIdentifier;

            var errorResponse = exception switch
            {
                // 400 — Validation error
                ValidationException ex => new ErrorResponseDto
                {
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    StatusCode = ex.StatusCode,
                    TraceId = traceId,
                    ValidationErrors = ex.Errors
                },

                // 404 — Not found
                NotFoundException ex => new ErrorResponseDto
                {
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    StatusCode = ex.StatusCode,
                    TraceId = traceId
                },

                // 404 — NAV data not found
                NavDataNotFoundException ex => new ErrorResponseDto
                {
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    StatusCode = ex.StatusCode,
                    TraceId = traceId
                },

                // 409 — Duplicate
                DuplicateException ex => new ErrorResponseDto
                {
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    StatusCode = ex.StatusCode,
                    TraceId = traceId
                },

                // 400 — Scheme enrollment
                SchemeEnrollmentException ex => new ErrorResponseDto
                {
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    StatusCode = ex.StatusCode,
                    TraceId = traceId
                },

                // 400 — Fund approval
                FundApprovalException ex => new ErrorResponseDto
                {
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    StatusCode = ex.StatusCode,
                    TraceId = traceId
                },

                // 500 — Base SchemeApi exception
                SchemeApiException ex => new ErrorResponseDto
                {
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    StatusCode = ex.StatusCode,
                    TraceId = traceId
                },

                // 500 — All other unexpected exceptions
                _ => new ErrorResponseDto
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "An unexpected error occurred. Please try again.",
                    StatusCode = 500,
                    TraceId = traceId
                }
            };

            // Log with appropriate level
            LogException(exception, errorResponse, context);

            context.Response.StatusCode = errorResponse.StatusCode;
            context.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition =
                    System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            await context.Response.WriteAsync(json);
        }

        private void LogException(
            Exception exception,
            ErrorResponseDto errorResponse,
            HttpContext context)
        {
            var request = $"{context.Request.Method} {context.Request.Path}";

            if (errorResponse.StatusCode >= 500)
            {
                _logger.LogCritical(exception,
                    "💥 Unhandled exception — TraceId: {TraceId} " +
                    "Request: {Request} ErrorCode: {ErrorCode}",
                    errorResponse.TraceId, request, errorResponse.ErrorCode);
            }
            else if (errorResponse.StatusCode >= 400)
            {
                _logger.LogWarning(
                    "⚠️ Client error — TraceId: {TraceId} " +
                    "Request: {Request} ErrorCode: {ErrorCode} Message: {Message}",
                    errorResponse.TraceId, request,
                    errorResponse.ErrorCode, errorResponse.Message);
            }
        }
    }

    // Extension method for clean registration in Program.cs
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(
            this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}