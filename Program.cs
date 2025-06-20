using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddFile("app.log");
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<JsonValidationMiddleware>();
app.UseMiddleware<JsonModificationMiddleware>();

app.MapControllers();

app.Run();
