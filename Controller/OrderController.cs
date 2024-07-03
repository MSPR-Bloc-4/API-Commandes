using Microsoft.AspNetCore.Mvc;
using Order_Api.Model;
using Order_Api.Service.Interface;

namespace Order_Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            try
            {
                string orderId = await _orderService.CreateOrder(order);
                return Ok(orderId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrder(string orderId)
        {
            try
            {
                Order order = await _orderService.GetOrderById(orderId);
                if (order == null)
                {
                    return NotFound();
                }
                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                List<Order> orders = await _orderService.GetAllOrders();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(string orderId, [FromBody] Order updatedOrder)
        {
            try
            {
                await _orderService.UpdateOrder(orderId, updatedOrder);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(string orderId)
        {
            try
            {
                await _orderService.DeleteOrder(orderId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
