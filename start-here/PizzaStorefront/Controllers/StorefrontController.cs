using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using PizzaShared.Messages.Storefront;
using PizzaStorefront.Services;

namespace PizzaStorefront.Controllers;

[ApiController]
[Route("[controller]")]
public class StorefrontController : ControllerBase
{
    private readonly IStorefrontService _storefrontService;
    private readonly ILogger<StorefrontController> _logger;
    private readonly DaprClient _daprClient;

    public StorefrontController(IStorefrontService storefrontService, ILogger<StorefrontController> logger, DaprClient daprClient)
    {
        _storefrontService = storefrontService;
        _logger = logger;
        _daprClient = daprClient;
    }

    [Topic("pizzapubsub", "storefront")]
    public async Task<ActionResult<OrderResultMessage>> CreateOrder(OrderMessage order)
    {
        _logger.LogInformation("Received new order: {OrderId}", order.OrderId);
        await _daprClient.PublishEventAsync("pizzapubsub", "workflow-storefront");
        return Ok();
    }
}