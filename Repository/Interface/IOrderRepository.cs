using Order_Api.Model;

namespace Order_Api.Repository.Interface;

public interface IOrderRepository
{
    Task<string> CreateOrder(Order order);
    Task<Order> GetOrderById(string orderId);
    Task<List<Order>> GetAllOrders();
    Task UpdateOrder(string orderId, Order order);
    Task DeleteOrder(string orderId);
    Task DeleteOrdersByUserId(string userId);
}