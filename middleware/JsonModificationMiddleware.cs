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

    // ---------------------------------------------------
    // main
    // ---------------------------------------------------
    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine("Middleware 3: JSON Modification");
        LoggerExtensions.LogInformation(_logger, "JsonModificationMiddleware: Starting JSON modification process.");

        try
        {
            string body = GetRequestBody(context);
            string modifiedJson = ProcessJsonBody(body);

            StoreModifiedBody(context, modifiedJson);
        }
        catch (Exception ex)
        {
            HandleJsonProcessingError(context, ex);
            return;
        }

        await _next(context);
    }

    // ---------------------------------------------------
    // Process
    // ---------------------------------------------------
    private string GetRequestBody(HttpContext context)
    {
        return context.Items["RequestBody"] as string;
    }

    private string ProcessJsonBody(string body)
    {
        var jsonObject = JsonConvert.DeserializeObject<dynamic>(body);
        jsonObject.timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        return JsonConvert.SerializeObject(jsonObject);
    }

    // ---------------------------------------------------
    // Save&Store Modified Body
    // ---------------------------------------------------
    private void StoreModifiedBody(HttpContext context, string modifiedJson)
    {
        context.Items["ModifiedBody"] = modifiedJson;
        LoggerExtensions.LogInformation(_logger, "JsonModificationMiddleware: Added timestamp to JSON. Modified body: {0}", modifiedJson);
    }

    // ---------------------------------------------------
    // Error Handler
    // ---------------------------------------------------
    private async Task HandleJsonProcessingError(HttpContext context, Exception ex)
    {
        string body = context.Items["RequestBody"] as string;
        _logger.LogError(ex, "JsonModificationMiddleware: Failed to modify JSON. Body: {0}", body);
        
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Error processing JSON");
    }
}