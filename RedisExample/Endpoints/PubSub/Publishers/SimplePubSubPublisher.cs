#region

using RedisExample.PubSubExamples.Events;
using StackExchange.Redis;
using System.Text.Json;

#endregion

namespace RedisExample.Endpoints.PubSub.Publishers;

public class SimplePubSubPublisher(RedisService redisService, ILogger<SimplePubSubPublisher> logger)
{
    private readonly ISubscriber _subscriber = redisService.GetSubscriber();

    public async Task PublishMessageAsync(string channel, string message)
    {
        // Mesajı yayınla ve kaç aboneye ulaştığını öğren
        var subscriberCount = await _subscriber.PublishAsync(
            new RedisChannel(channel, RedisChannel.PatternMode.Literal),
            message
        );

        logger.LogInformation(
            "Mesaj '{Message}' kanalına '{Channel}' yayınlandı. {Count} abone aldı.",
            message, channel, subscriberCount
        );
    }

    public async Task PublishJsonMessageAsync<T>(string channel, T data)
    {
        var json = JsonSerializer.Serialize(data);
        await PublishMessageAsync(channel, json);
    }

    public async Task PublishUserCreatedEventAsync(UserCreatedEvent userEvent)
    {
        var json = JsonSerializer.Serialize(userEvent, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        var subscriberCount = await _subscriber.PublishAsync(
            new RedisChannel("user.created", RedisChannel.PatternMode.Literal),
            json
        );

        logger.LogInformation(
            "🎉 UserCreated event published - UserId: {UserId}, UserName: {UserName}, Subscribers: {Count}",
            userEvent.UserId, userEvent.UserName, subscriberCount
        );
    }
}