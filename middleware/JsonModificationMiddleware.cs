using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

using static ErrorResponseHelper;

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
        if (context.Items.TryGetValue("ValidationFailed", out var failed) && (failed as bool?) == true)
        {
            await _next(context);
            return;
        }
        
        Console.WriteLine("Middleware 3: JSON Modification");
        LoggerExtensions.LogInformation(_logger, "JsonModificationMiddleware: Starting JSON modification process.");

        try
        {
            var body = GetRequestBody(context);
            var modifiedJson = ProcessJsonBody(body);

            StoreModifiedBody(context, modifiedJson);
        }
        catch (Exception ex)
        {
            await HandleJsonProcessingError(context, ex, _logger, context.Items["RequestBody"] as string);
            return;
        }

        await _next(context);
    }

    // ---------------------------------------------------
    // Process
    // ---------------------------------------------------
    private JObject? GetRequestBody(HttpContext context)
    {
        return context.Items["ParsedJson"] as JObject;
    }

    private string ProcessJsonBody(JObject? body)
    {
        body.Add("timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        return JsonConvert.SerializeObject(body);
    }

    // ---------------------------------------------------
    // Save&Store Modified Body
    // ---------------------------------------------------
    private void StoreModifiedBody(HttpContext context, string modifiedJson)
    {
        context.Items["ModifiedBody"] = modifiedJson;
        LoggerExtensions.LogInformation(_logger, "JsonModificationMiddleware: Added timestamp to JSON. Modified body: {0}", modifiedJson);
    }
}