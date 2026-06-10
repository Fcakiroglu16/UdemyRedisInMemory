#region

using StackExchange.Redis;
using System.Text.Json;

#endregion

namespace RedisExample.Endpoints.PubSub.EventDrivenArchitecture;

public class EventBus(RedisService redisService, ILogger<EventBus> logger)
{
    private readonly ISubscriber _subscriber = redisService.GetSubscriber();

    [Obsolete]
    public async Task PublishAsync<T>(T @event) where T : IEvent
    {
        var eventType = @event.GetType().Name;
        var channel = $"events.{eventType}";
        var json = JsonSerializer.Serialize(@event);

        var count = await _subscriber.PublishAsync(channel, json);
        logger.LogInformation(
            "📤 Event yayınlandı: {EventType}, {Count} servis aldı",
            eventType, count
        );
    }

    [Obsolete]
    public async Task SubscribeAsync<T>(Func<T, Task> handler) where T : IEvent
    {
        var eventType = typeof(T).Name;
        var channel = $"events.{eventType}";

        await _subscriber.SubscribeAsync(channel, async (ch, message) =>
        {
            try
            {
                var @event = JsonSerializer.Deserialize<T>(message!);
                await handler(@event!);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Event işlenirken hata: {EventType}", eventType);
            }
        });

        logger.LogInformation("👂 Event dinleniyor: {EventType}", eventType);
    }
}

// Event marker interface
public interface IEvent
{
    string EventId { get; }
    DateTime OccurredAt { get; }
}

// Event örnekleri
public record OrderCreatedEvent(string OrderId, decimal Total) : IEvent
{
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record PaymentProcessedEvent(string PaymentId, string OrderId, bool Success) : IEvent
{
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record InventoryUpdatedEvent(string ProductId, int NewQuantity) : IEvent
{
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record UserRegisteredEvent(string UserId, string Email) : IEvent
{
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
