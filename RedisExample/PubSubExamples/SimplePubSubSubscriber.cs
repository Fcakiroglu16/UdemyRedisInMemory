#region

using RedisExample.PubSubExamples.Events;
using StackExchange.Redis;
using System.Text.Json;

#endregion

namespace RedisExample.PubSubExamples;

public class SimplePubSubSubscriber(RedisService redisService, ILogger<SimplePubSubSubscriber> logger)
    : BackgroundService
{
    private readonly ISubscriber _subscriber = redisService.GetSubscriber();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Kanala abone ol
        await _subscriber.SubscribeAsync(
            new RedisChannel("notifications", RedisChannel.PatternMode.Literal),
            (channel, message) =>
            {
                logger.LogInformation(
                    "📨 Mesaj alındı - Kanal: {Channel}, İçerik: {Message}",
                    channel, message
                );

                // Mesajı işle
                ProcessMessage(message!);
            }
        );

        logger.LogInformation("✅ 'notifications' kanalına abone olundu");

        // UserCreated event'ına abone ol
        await _subscriber.SubscribeAsync(
            new RedisChannel("user.created", RedisChannel.PatternMode.Literal),
            (channel, message) =>
            {
                logger.LogInformation(
                    "📨 UserCreated event alındı - Kanal: {Channel}",
                    channel
                );

                // Event'ı işle
                ProcessUserCreatedEvent(message!);
            }
        );

        logger.LogInformation("✅ 'user.created' kanalına abone olundu");

        // Background service çalışmaya devam etsin
      //  await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void ProcessMessage(string message)
    {
        // İş mantığı burada
        logger.LogInformation("⚙️ Mesaj işleniyor: {Message}", message);
    }

    private void ProcessUserCreatedEvent(string message)
    {
        try
        {
            var userEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);
            
            if (userEvent != null)
            {
                Console.WriteLine("╔════════════════════════════════════════╗");
                Console.WriteLine("║     🎉 USER CREATED EVENT ALINDI     ║");
                Console.WriteLine("╚════════════════════════════════════════╝");
                Console.WriteLine($"👤 User ID       : {userEvent.UserId}");
                Console.WriteLine($"📝 User Name     : {userEvent.UserName}");
                Console.WriteLine($"📧 Email         : {userEvent.Email}");
                Console.WriteLine($"🎭 Role          : {userEvent.Role}");
                Console.WriteLine($"📅 Created At    : {userEvent.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine("════════════════════════════════════════");
                
                logger.LogInformation(
                    "⚙️ UserCreated event işlendi - UserId: {UserId}, UserName: {UserName}",
                    userEvent.UserId, userEvent.UserName
                );
            }
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "❌ UserCreated event deserialize edilemedi");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // Abonelikten çık
        await _subscriber.UnsubscribeAsync(new RedisChannel("notifications", RedisChannel.PatternMode.Literal));
        logger.LogWarning("⚠️ 'notifications' kanalından abonelik iptal edildi");

        await _subscriber.UnsubscribeAsync(new RedisChannel("user.created", RedisChannel.PatternMode.Literal));
        logger.LogWarning("⚠️ 'user.created' kanalından abonelik iptal edildi");

        await base.StopAsync(cancellationToken);
    }
}