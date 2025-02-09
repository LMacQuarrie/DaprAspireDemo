using Dapr.Workflow;
using PizzaWorkflow.Models;
using PizzaWorkflow.Activities;
using Microsoft.Extensions.Logging;

namespace PizzaWorkflow.Workflows;

public class PizzaOrderingWorkflow : Workflow<Order, Order>
{
    public override async Task<Order> RunAsync(WorkflowContext context, Order order)
    {
        try
        {
            // Step 1: Place and process the order
            await context.CallActivityAsync<Order>(
              nameof(StorefrontActivity),
              order);

            var orderResult = await context.WaitForExternalEventAsync<Order>("OrderComplete");

            if (orderResult.Status != "confirmed")
            {
                throw new Exception($"Order failed: {orderResult.Error ?? "Unknown error"}");
            }

            // Step 2: Cook the pizza
            await context.CallActivityAsync<Order>(
              nameof(CookingActivity),
              orderResult);

            var cookingResult = await context.WaitForExternalEventAsync<Order>("CookComplete");

            if (cookingResult.Status != "cooked")
            {
                throw new Exception($"Cooking failed: {cookingResult.Error ?? "Unknown error"}");
            }

            // Update status to waiting for validation
            cookingResult.Status = "waiting_for_validation";
            await context.CallActivityAsync<Order>(
              nameof(ValidationActivity),
              cookingResult);

            // Step 3: Wait for manager validation
            var validationEvent = await context.WaitForExternalEventAsync<ValidationRequest>("ValidationComplete");

            if (!validationEvent.Approved)
            {
                throw new Exception("Pizza validation failed - need to remake");
            }

            // Step 4: Deliver the pizza
            await context.CallActivityAsync<Order>(
              nameof(DeliveryActivity),
              cookingResult);

            var deliveryResult = await context.WaitForExternalEventAsync<Order>("DeliveryComplete");

            if (deliveryResult.Status != "delivered")
            {
                throw new Exception($"Delivery failed: {deliveryResult.Error ?? "Unknown error"}");
            }

            deliveryResult.Status = "completed";
            return deliveryResult;
        }
        catch (Exception ex)
        {
            order.Status = "failed";
            order.Error = ex.Message;
            return order;
        }
    }
}
