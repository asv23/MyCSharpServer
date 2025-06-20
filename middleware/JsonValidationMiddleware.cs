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

    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine("Middleware 2: JSON Validation");

        if (context.Request.Method == "POST" && context.Request.Path == "/messager")
        {
            context.Request.EnableBuffering();

            try
            {
                using var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: false);
                
                var body = await reader.ReadToEndAsync();

                if (string.IsNullOrEmpty(body))
                {
                    _logger.LogError("JsonValidationMiddleware: Request body is empty.");
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("{\"error\": \"Request body is empty.\"}");
                    return;
                }

                var jsonObject = JsonConvert.DeserializeObject(body);
                _logger.LogInformation("JsonValidationMiddleware: Successfully parsed JSON. Body: {body}", body);

                context.Items["RequestBody"] = body;

                var newBodyStream = new MemoryStream(Encoding.UTF8.GetBytes(body));
                context.Request.Body = newBodyStream;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JsonValidationMiddleware: Failed to parse JSON from request body");
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("{\"error\": \"Invalid JSON format.\"}");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JsonValidationMiddleware: Unexpected error while processing request body");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("{\"error\": \"Internal server error.\"}");
                return;
            }
        }
        await _next(context);
    }
}