using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;  

public static class ErrorResponseHelper
{
    // ---------------------------------------------------
    // Error Responser
    // ---------------------------------------------------
    private static async Task ResponseWithCode(HttpContext context, ILogger logger, int statusCode, string title, string message, Exception? error = null, string? details = null)
    {
        if (error != null && details != null)
        {
            logger.LogError(error, title, details);
        }
        else if (error != null)
        {
            logger.LogError(error, title);
        }
        else
        {
            logger.LogError(title);
        }
                
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = message }));
    }

    // ---------------------------------------------------
    // Specific Error
    // ---------------------------------------------------
    public static async Task HandleJsonParseError(HttpContext context, Exception ex, ILogger logger)
    {
        if (ex is JsonException)
        {
            const string title = "JsonValidationMiddleware: Failed to parse JSON from request body.";
            const string message = "Invalid JSON format.";

            await ResponseWithCode(context, logger, StatusCodes.Status400BadRequest, title, message, ex);
        }
        else
        {
            const string title = "JsonValidationMiddleware: Unexpected error while processing request body.";
            const string message = "Internal server error.";

            await ResponseWithCode(context, logger, StatusCodes.Status500InternalServerError, title, message, ex);
        }
    }

    public static async Task HandleEmptyBodyError(HttpContext context, ILogger logger)
    {
        const string title = "JsonValidationMiddleware: Request body is empty.";
        const string message = "Request body is empty.";

        await ResponseWithCode(context, logger, StatusCodes.Status422UnprocessableEntity, title, message);
    }

    public static async Task HandleJsonProcessingError(HttpContext context, Exception ex, ILogger logger, string details = null)
    {
        const string title = "JsonValidationMiddleware: Unexpected error while processing request body.";
        const string message = "Internal server error.";

        await ResponseWithCode(context, logger, StatusCodes.Status500InternalServerError, title, message, ex, details);
    }
}