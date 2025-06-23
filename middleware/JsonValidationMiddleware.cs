using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class JsonValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JsonValidationMiddleware> _logger;

    public JsonValidationMiddleware(RequestDelegate next, ILogger<JsonValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }


    // ---------------------------------------------------
    // main
    // ---------------------------------------------------
    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine("Middleware 2: JSON Validation");

        if (ShouldProcessRequest(context))
        {
            var continueProcess = await ProcessRequest(context);
            if(!continueProcess) 
            {
                context.Items["ValidationFailed"] = true;
                return;
            }
        }
        else
        {
            context.Items["ValidationFailed"] = true;
            return;
        }

        await _next(context);
    }

    // ---------------------------------------------------
    // Check
    // ---------------------------------------------------
    private bool ShouldProcessRequest(HttpContext context)
    {
        return (context.Request.Method == "POST" && context.Request.Path == "/messager");
    }

    // ---------------------------------------------------
    // Process Request
    // ---------------------------------------------------
    private async Task<bool> ProcessRequest(HttpContext context)
    {
        context.Request.EnableBuffering();

        string body = await ReadRequestBody(context.Request.Body);

        if(string.IsNullOrEmpty(body))
        {
            await HandleEmptyBodyError(context);
            return false;
        }

        try
        {
            TryProcessJson(context, body);
        }
        catch(Exception ex)
        {
            await HandleJsonParseError(context, ex);
            return false;
        }
        return true;
    }

    // ---------------------------------------------------
    // Reader
    // ---------------------------------------------------
    private async Task<string> ReadRequestBody(Stream requestBody)
    {
        using var reader = new StreamReader(
            requestBody,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: false);
        
        return await reader.ReadToEndAsync();
    }

    // ---------------------------------------------------
    // Custom Error Handler
    // ---------------------------------------------------
    private async Task HandleEmptyBodyError(HttpContext context)
    {
        _logger.LogError("JsonValidationMiddleware: Request body is empty.");
        context.Response.StatusCode = 422;
        await context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = "Request body is empty." }));
        // await context.Response.WriteAsync("{\"error\": \"Request body is empty.\"}");
    }
    
    private async Task HandleJsonParseError(HttpContext context, Exception error)
    {
        if (error is JsonException)
        {
            _logger.LogError(error, "JsonValidationMiddleware: Failed to parse JSON from request body");
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = "Invalid JSON format." }));
            // await context.Response.WriteAsync("{\"error\": \"Invalid JSON format.\"}");
        }
        else
        {
            _logger.LogError(error, "JsonValidationMiddleware: Unexpected error while processing request body");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = "Internal server error." }));
            // await context.Response.WriteAsync("{\"error\": \"Internal server error.\"}");
        }
    }

    // ---------------------------------------------------
    // Valid Json Process
    // ---------------------------------------------------
    private void TryProcessJson(HttpContext context, string json)
    {
        var jsonObject = JsonConvert.DeserializeObject(json);
        context.Items["ParsedJson"] = jsonObject;
        _logger.LogInformation("JsonValidationMiddleware: Successfully parsed JSON. Body: {body}", json);

        // var newBodyStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        // context.Request.Body = newBodyStream;
    }
}