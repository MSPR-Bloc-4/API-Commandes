using Google.Cloud.PubSub.V1;

namespace Order_Api.Service;

public class SubscriberService : BackgroundService
{
    private readonly SubscriberClient _subscriberClient;
    private readonly ILogger<SubscriberService> _logger;

    public SubscriberService(SubscriberClient subscriberClient, ILogger<SubscriberService> logger)
    {
        _subscriberClient = subscriberClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
        await _subscriberClient.StartAsync((msg, token) =>
        {
            _logger.LogInformation($"Received message {msg.MessageId}: {msg.Data.ToStringUtf8()}");
            // Handle the message.
            return Task.FromResult(SubscriberClient.Reply.Ack);
        });

    public override async Task StopAsync(CancellationToken stoppingToken) =>
        await _subscriberClient.StopAsync(stoppingToken);
}