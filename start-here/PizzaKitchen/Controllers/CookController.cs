using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using PizzaKitchen.Services;
using PizzaShared.Messages.Kitchen;

namespace PizzaKitchen.Controllers;

[ApiController]
[Route("[controller]")]
public class CookController : ControllerBase
{
    private readonly ICookService _cookService;
    private readonly ILogger<CookController> _logger;
    private readonly DaprClient _daprClient;

    public CookController(ICookService cookService, ILogger<CookController> logger, DaprClient daprClient)
    {
        _cookService = cookService;
        _logger = logger;
        _daprClient = daprClient;
    }

    [Topic("pizzapubsub", "kitchen")]
    public async Task<ActionResult<CookResultMessage>> Cook(CookMessage cookMessage)
    {
        _logger.LogInformation("Starting cooking for order: {OrderId}", cookMessage.OrderId);
        var result = await _cookService.CookPizzaAsync(cookMessage);
        
        await _daprClient.PublishEventAsync("pizzapubsub", "workflow-kitchen", result);

        return Ok();
    }
}