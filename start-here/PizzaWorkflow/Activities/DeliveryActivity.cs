using Dapr.Client;
using Dapr.Workflow;
using Microsoft.VisualBasic.CompilerServices;
using PizzaShared.Messages.Delivery;
using PizzaWorkflow.Models;

namespace PizzaWorkflow.Activities;

public class DeliveryActivity : WorkflowActivity<Order, Object?>
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<DeliveryActivity> _logger;

    public DeliveryActivity(DaprClient daprClient, ILogger<DeliveryActivity> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    public override async Task<Object?> RunAsync(WorkflowActivityContext context, Order order)
    {
        try
        {
            _logger.LogInformation("Starting delivery process for order {OrderId}", order.OrderId);

            var message = MessageHelper.FillMessage<DeliveryMessage>(context, order);

            await _daprClient.PublishEventAsync("pizzapubsub", "delivery", message);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delivering order {OrderId}", order.OrderId);
            throw;
        }
    }
}
