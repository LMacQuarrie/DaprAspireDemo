using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using PizzaDelivery.Services;
using PizzaShared.Messages.Delivery;

namespace PizzaDelivery.Controllers;

[ApiController]
[Route("[controller]")]
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;
    private readonly ILogger<DeliveryController> _logger;
    private readonly DaprClient _daprClient;

    public DeliveryController(IDeliveryService deliveryService, ILogger<DeliveryController> logger, DaprClient daprClient)
    {
        _deliveryService = deliveryService;
        _logger = logger;
        _daprClient = daprClient;
    }

    [Topic("pizzapubsub", "delivery")]
    public async Task<ActionResult<DeliveryResultMessage>> Deliver(DeliveryMessage deliveryMessage)
    {
        _logger.LogInformation("Starting delivery for order: {OrderId}", deliveryMessage.OrderId);
        var result = await _deliveryService.DeliverPizzaAsync(deliveryMessage);

        await _daprClient.PublishEventAsync("pizzapubsub", "workflow-delivery", result);

        return Ok();
    }
}