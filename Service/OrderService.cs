using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Order_Api.Configuration;
using Order_Api.Model;
using Order_Api.Repository.Interface;
using Order_Api.Service.Interface;

namespace Order_Api.Service;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly PublisherServiceApiClient _publisherClient;
    private readonly FirebaseConfig _firebaseConfig;

    public OrderService(IOrderRepository orderRepository, IOptions<FirebaseConfig> firebaseConfig)
    {
        _orderRepository = orderRepository;
        _firebaseConfig = firebaseConfig.Value;
        _publisherClient = new PublisherServiceApiClientBuilder
        {
            CredentialsPath = _firebaseConfig.ServiceAccountPath
        }.Build();    }

    public async Task<string> CreateOrder(Order order)
    {
        var result = await _orderRepository.CreateOrder(order);
        TopicName topicName = new TopicName(_firebaseConfig.ProjectId, "order-created");

        string message = string.Join(",", order.Products);
        PubsubMessage pubsubMessage = new PubsubMessage
        {
            Data = ByteString.CopyFromUtf8(message)
        };
        await _publisherClient.PublishAsync(topicName, new[] { pubsubMessage });          

        return result;
    }

    public async Task<Order> GetOrderById(string orderId)
    {
        return await _orderRepository.GetOrderById(orderId);
    }

    public async Task<List<Order>> GetAllOrders()
    {
        return await _orderRepository.GetAllOrders();
    }

    public async Task UpdateOrder(string orderId, Order order)
    {
        await _orderRepository.UpdateOrder(orderId, order);
    }

    public async Task DeleteOrder(string orderId)
    {
        await _orderRepository.DeleteOrder(orderId);
    }

    public async Task DeleteOrdersByUserId(string userId)
    {
        TopicName topicName = new TopicName(_firebaseConfig.ProjectId, "order-deleted");
        var orders = await _orderRepository.GetOrdersByUserId(userId);
        var productIds = orders.SelectMany(o => o.Products).ToList();
        var orderIds = orders.Select(o => o.Id).ToList();
        await _orderRepository.DeleteOrdersByIds(orderIds);

        // Publish the order IDs to the "order-deleted" topic
        if (orderIds.Any())
        {
            string message = string.Join(",", productIds);
            PubsubMessage pubsubMessage = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(message)
            };
            await _publisherClient.PublishAsync(topicName, new[] { pubsubMessage });          
        }
    }
}