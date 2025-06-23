using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("messager")]
public class MessagerController : ControllerBase
{
    private readonly ILogger<MessagerController> _logger;

    public MessagerController(ILogger<MessagerController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult Post()
    {
        if (!HttpContext.Items.TryGetValue("ModifiedBody", out var modifiedBody) || modifiedBody == null)
        {
            _logger.LogError("MessagerController: Failed to retrieve modified JSON");
            return StatusCode(500, "Internal server error");
        }
        _logger.LogInformation("MessagerController: Returning modified JSON to client")
        return Content(modifiedBody.ToString()!, "application/json");
    }
}