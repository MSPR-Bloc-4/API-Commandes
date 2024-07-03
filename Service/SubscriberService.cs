using Google.Cloud.PubSub.V1;
using Order_Api.Service.Interface;

namespace Order_Api.Service;

public class SubscriberService : BackgroundService
{
    private readonly SubscriberClient _subscriberClient;
    private readonly ILogger<SubscriberService> _logger;
    public readonly IOrderService _orderService;

    public SubscriberService(SubscriberClient subscriberClient, ILogger<SubscriberService> logger, IOrderService orderService)
    {
        _subscriberClient = subscriberClient;
        _logger = logger;
        _orderService = orderService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
        await _subscriberClient.StartAsync((msg, token) =>
        {
            _orderService.DeleteOrdersByUserId(userId: msg.Data.ToStringUtf8());
            _logger.LogInformation($"Received message {msg.MessageId}: {msg.Data.ToStringUtf8()}");
            return Task.FromResult(SubscriberClient.Reply.Ack);
        });

    public override async Task StopAsync(CancellationToken stoppingToken) =>
        await _subscriberClient.StopAsync(stoppingToken);
}