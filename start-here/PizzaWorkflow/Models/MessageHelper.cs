using Dapr.Workflow;
using Google.Api;
using PizzaShared.Messages;

namespace PizzaWorkflow.Models
{
    public class MessageHelper
    {
        public static T FillMessage<T>(WorkflowActivityContext context, Order order) where T : WorkflowMessage, new()
        {
            var result = new T();

            var customer = new CustomerDto
            {
                Address = order.Customer.Address,
                Name = order.Customer.Name,
                Phone = order.Customer.Phone
            };

            var examType = typeof(T);
            // Change the instance property value.
            var workflowId = examType.GetProperty("WorkflowId");
            workflowId.SetValue(result, context.InstanceId);

            // Change the instance property value.
            var orderId = examType.GetProperty("OrderId");
            orderId.SetValue(result, order.OrderId);

            // Change the instance property value.
            var pizzaType = examType.GetProperty("PizzaType");
            pizzaType.SetValue(result, order.PizzaType);

            // Change the instance property value.
            var size = examType.GetProperty("Size");
            size.SetValue(result, order.Size);

            // Change the instance property value.
            var customerProp = examType.GetProperty("Customer");
            customerProp.SetValue(result, customer);

            return result;
        }

        public static Order FillOrder<T>(T message) where T : WorkflowMessage
        {
            var examType = typeof(T);

            // workflowId = (string)examType.GetProperty("WorkflowId").GetValue(message);

            var orderId = (string)examType.GetProperty("OrderId").GetValue(message);
            var pizzaType = (string)examType.GetProperty("PizzaType").GetValue(message);
            var size = (string)examType.GetProperty("Size").GetValue(message);
            var status = (string)examType.GetProperty("Status").GetValue(message);
            var error = (string)examType.GetProperty("Error").GetValue(message) ?? string.Empty;

            var customerDto = (CustomerDto)examType.GetProperty("Customer").GetValue(message);

            var customer = new Customer
            {
                Address = customerDto.Address,
                Name = customerDto.Name,
                Phone = customerDto.Phone
            };

            var result = new Order
            {
                OrderId = orderId,
                PizzaType = pizzaType,
                Size = size,
                Customer = customer,
                Status = status,
                Error = error
            };

            return result;
        }
    }
}
