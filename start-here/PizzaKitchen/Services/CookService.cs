using Dapr.Client;
using PizzaShared.Messages.Kitchen;
using PizzaShared.Messages.Storefront;

namespace PizzaKitchen.Services;

public interface ICookService
{
    Task<CookResultMessage> CookPizzaAsync(CookMessage cookMessage);
}

public class CookService : ICookService
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<CookService> _logger;

    private const string PUBSUB_NAME = "pizzapubsub";
    private const string TOPIC_NAME = "orders";


    public CookService(DaprClient daprClient, ILogger<CookService> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    public async Task<CookResultMessage> CookPizzaAsync(CookMessage cookMessage)
    {
        var stages = new (string status, int duration)[]
        {
            ("cooking_preparing_ingredients", 2),
            ("cooking_making_dough", 3),
            ("cooking_adding_toppings", 2),
            ("cooking_baking", 5),
            ("cooking_quality_check", 1)
        };

        var order = new CookResultMessage
        {
            WorkflowId = cookMessage.WorkflowId,
            OrderId = cookMessage.OrderId,
            PizzaType = cookMessage.PizzaType,
            Size = cookMessage.Size,
            Customer = cookMessage.Customer,
            Status = "unknown"
        };

        try
        {
            foreach (var (status, duration) in stages)
            {
                order.Status = status;
                _logger.LogInformation("Order {OrderId} - {Status}", order.OrderId, status);

                await _daprClient.PublishEventAsync(PUBSUB_NAME, TOPIC_NAME, order);
                await Task.Delay(TimeSpan.FromSeconds(duration));
            }

            order.Status = "cooked";
            await _daprClient.PublishEventAsync(PUBSUB_NAME, TOPIC_NAME, order);
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cooking order {OrderId}", order.OrderId);
            order.Status = "cooking_failed";
            order.Error = ex.Message;
            await _daprClient.PublishEventAsync(PUBSUB_NAME, TOPIC_NAME, order);
            return order;
        }
    }
}