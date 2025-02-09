using Microsoft.AspNetCore.Mvc;
using PizzaOrder.Models;
using PizzaOrder.Services;

namespace PizzaOrder.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderStateService _orderStateService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderStateService orderStateService, ILogger<OrderController> logger)
    {
        _orderStateService = orderStateService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(Order order)
    {
        _logger.LogInformation("Received new order: {OrderId}", order.OrderId);
        var result = await _orderStateService.UpdateOrderStateAsync(order);
        return Ok(result);
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<Order>> GetOrder(string orderId)
    {
        var order = await _orderStateService.GetOrderAsync(orderId);
        
        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    
    [HttpDelete("{orderId}")]
    public async Task<ActionResult<string>> DeleteOrder(string orderId)
    {
        var order = await _orderStateService.GetOrderAsync(orderId);
        
        if (order == null)
        {
            return NotFound();
        }

        await _orderStateService.DeleteOrderAsync(orderId);
        return Ok(orderId);
    }

    [HttpPost("/orders-sub")]
    public async Task<IActionResult> HandleOrderUpdate(Order cloudEvent)
    {
        _logger.LogInformation("Received order update for order {OrderId}", 
            cloudEvent.OrderId);

        var result = await _orderStateService.UpdateOrderStateAsync(cloudEvent);
        return Ok();
    }
}