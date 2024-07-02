using Microsoft.AspNetCore.Mvc;
using Order_Api.Model;
using Order_Api.Repository.Interface;

namespace Order_Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            try
            {
                string orderId = await _orderRepository.CreateOrder(order);
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
                Order order = await _orderRepository.GetOrderById(orderId);
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
                List<Order> orders = await _orderRepository.GetAllOrders();
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
                await _orderRepository.UpdateOrder(orderId, updatedOrder);
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
                await _orderRepository.DeleteOrder(orderId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
