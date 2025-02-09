using Dapr.Client;
using Dapr.Workflow;
using PizzaShared.Messages.Storefront;
using PizzaWorkflow.Models;

namespace PizzaWorkflow.Activities;

public class StorefrontActivity : WorkflowActivity<Order, Object?>
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<StorefrontActivity> _logger;

    public StorefrontActivity(DaprClient daprClient, ILogger<StorefrontActivity> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    public override async Task<Object?> RunAsync(WorkflowActivityContext context, Order order)
    {
        try
        {
            _logger.LogInformation("Starting ordering process for order {OrderId}", order.OrderId);

            var message = MessageHelper.FillMessage<OrderMessage>(context, order);

            await _daprClient.PublishEventAsync("pizzapubsub", "storefront", message);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order {OrderId}", order.OrderId);
            throw;
        }
    }
}
