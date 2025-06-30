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
    // Valid Json Process
    // ---------------------------------------------------
    private void TryProcessJson(HttpContext context, string json)
    {
        var jsonObject = JsonConvert.DeserializeObject(json);
        context.Items["ParsedJson"] = jsonObject;
        _logger.LogInformation("JsonValidationMiddleware: Successfully parsed JSON. Body: {body}", json);
    }
}