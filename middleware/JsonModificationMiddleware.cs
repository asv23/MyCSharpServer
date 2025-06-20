using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

public class JsonModificationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JsonModificationMiddleware> _logger;

    public JsonModificationMiddleware(RequestDelegate next, ILogger<JsonModificationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine("Middleware 3: JSON Modification");
        LoggerExtensions.LogInformation(_logger, "JsonModificationMiddleware: Starting JSON modification process.");

        if (context.Request.Method == "POST" && context.Request.Path == "/messager")
        {
            if (context.Items.TryGetValue("RequestBody", out var bodyObj) && bodyObj is string body)
            {
                if (string.IsNullOrEmpty(body))
                {
                    LoggerExtensions.LogWarning(_logger, "JsonModificationMiddleware: Request body is empty or null.");
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Request body is empty");
                    return;
                }

                try
                {
                    var jsonObject = JsonConvert.DeserializeObject<dynamic>(body);
                    if (jsonObject == null)
                    {
                        LoggerExtensions.LogError(_logger, "JsonModificationMiddleware: Failed to deserialize JSON. Body: {0}", body);
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("Invalid JSON format");
                        return;
                    }

                    jsonObject.timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                    var modifiedJson = JsonConvert.SerializeObject(jsonObject);
                    if (modifiedJson == null)
                    {
                        LoggerExtensions.LogError(_logger, "JsonModificationMiddleware: Failed to serialize modified JSON.");
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("Error serializing JSON");
                        return;
                    }

                    context.Items["ModifiedBody"] = modifiedJson;
                    LoggerExtensions.LogInformation(_logger, "JsonModificationMiddleware: Added timestamp to JSON. Modified body: {0}", modifiedJson);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "JsonModificationMiddleware: Failed to modify JSON. Body: {0}", body);
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Error processing JSON");
                    return;
                }
            }
            else
            {
                LoggerExtensions.LogWarning(_logger, "JsonModificationMiddleware: Request body not found in context.Items.");
            }
        }
        await _next(context);
    }
}