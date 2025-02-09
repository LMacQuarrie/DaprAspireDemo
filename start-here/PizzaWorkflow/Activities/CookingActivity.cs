using Dapr.Workflow;
using Dapr.Client;
using PizzaShared.Messages.Kitchen;
using PizzaWorkflow.Models;

namespace PizzaWorkflow.Activities
{
    public class CookingActivity : WorkflowActivity<Order, Object?>
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger<CookingActivity> _logger;

        public CookingActivity(DaprClient daprClient, ILogger<CookingActivity> logger)
        {
            _daprClient = daprClient;
            _logger = logger;
        }

        public override async Task<Object?> RunAsync(WorkflowActivityContext context, Order order)
        {
            try
            {
                _logger.LogInformation("Starting cooking process for order {OrderId}", order.OrderId);

                var message = MessageHelper.FillMessage<CookMessage>(context, order);

                await _daprClient.PublishEventAsync("pizzapubsub", "kitchen", message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cooking order {OrderId}", order.OrderId);
                throw;
            }
        }
    }
}
