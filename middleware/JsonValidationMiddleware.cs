using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using static ErrorResponseHelper;

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
            await HandleEmptyBodyError(context, _logger);
            return false;
        }

        try
        {
            TryProcessJson(context, body);
        }
        catch(Exception ex)
        {
            await HandleJsonParseError(context, ex, _logger);
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
    // private async Task HandleEmptyBodyError(HttpContext context)
    // {
    //     // _logger.LogError("JsonValidationMiddleware: Request body is empty.");
    //     // context.Response.StatusCode = 422;
    //     // await context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = "Request body is empty." }));

    //     int statusCode = 422;
    //     string title = "JsonValidationMiddleware: Request body is empty.";
    //     string message = "Request body is empty.";

    //     ResponseWithCode(context, _logger, statusCode, title, message);
    // }
    
    // private async Task HandleJsonParseError(HttpContext context, Exception ex)
    // {
    //     if (ex is JsonException)
    //     {
    //         // _logger.LogError(error, "JsonValidationMiddleware: Failed to parse JSON from request body.");
    //         // context.Response.StatusCode = 400;
    //         // await context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = "Invalid JSON format." }));
            
    //         int statusCode = 400;
    //         string title = "JsonValidationMiddleware: Failed to parse JSON from request body.";
    //         string message = "Invalid JSON format.";

    //         ResponseWithCode(context, _logger, statusCode, title, message, ex);
    //     }
    //     else
    //     {
    //         // _logger.LogError(error, "JsonValidationMiddleware: Unexpected error while processing request body.");
    //         // context.Response.StatusCode = 500;
    //         // await context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = "Internal server error." }));

    //         int statusCode = 500;
    //         string title = "JsonValidationMiddleware: Unexpected error while processing request body.";
    //         string message = "Internal server error.";

    //         ResponseWithCode(context, _logger, statusCode, title, message, ex);
    //     }
    // }

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