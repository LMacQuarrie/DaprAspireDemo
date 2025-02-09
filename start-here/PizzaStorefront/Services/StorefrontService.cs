using Dapr.Client;
using PizzaShared.Messages.Storefront;

namespace PizzaStorefront.Services;

public interface IStorefrontService
{
    Task<OrderResultMessage> ProcessOrderAsync(OrderMessage orderMessage);
}

public class StorefrontService : IStorefrontService
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<StorefrontService> _logger;

    private const string PUBSUB_NAME = "pizzapubsub";
    private const string TOPIC_NAME = "orders";

    public StorefrontService(DaprClient daprClient, ILogger<StorefrontService> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    public async Task<OrderResultMessage> ProcessOrderAsync(OrderMessage orderMessage)
    {
        var stages = new (string status, int duration)[]
        {
            ("validating", 1),
            ("processing", 2),
            ("confirmed", 1)
        };

        var order = new OrderResultMessage
        {
            WorkflowId = orderMessage.WorkflowId,
            OrderId = orderMessage.OrderId,
            PizzaType = orderMessage.PizzaType,
            Size = orderMessage.Size,
            Customer = orderMessage.Customer,
            Status = "unknown"
        };

        try
        {
            // Set pizza order status
            foreach (var (status, duration) in stages)
            {
                order.Status = status;
                _logger.LogInformation("Order {OrderId} - {Status}", order.OrderId, status);

                await _daprClient.PublishEventAsync(PUBSUB_NAME, TOPIC_NAME, order);
                await Task.Delay(TimeSpan.FromSeconds(duration));
            }

            //_logger.LogInformation("Starting cooking process for order {OrderId}", order.OrderId);

            //// Use the Service Invocation building block to invoke the endpoint in the pizza-kitchen service
            //var response = await _daprClient.InvokeMethodAsync<Order, Order>(
            //    HttpMethod.Post,
            //    "pizza-kitchen",
            //    "cook",
            //    order);

            //_logger.LogInformation("Order {OrderId} cooked with status {Status}",
            //    order.OrderId, response.Status);

            //// Use the Service Invocation building block to invoke the endpoint in the pizza-delivery service
            //_logger.LogInformation("Starting delivery process for order {OrderId}", order.OrderId);

            //response = await _daprClient.InvokeMethodAsync<Order, Order>(
            //    HttpMethod.Post,
            //    "pizza-delivery",
            //    "delivery",
            //    order);

            //_logger.LogInformation("Order {OrderId} delivered with status {Status}",
            //    order.OrderId, response.Status);

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order {OrderId}", order.OrderId);
            order.Status = "failed";
            order.Error = ex.Message;

            await _daprClient.PublishEventAsync(PUBSUB_NAME, TOPIC_NAME, order);
            return order;
        }
    }
}