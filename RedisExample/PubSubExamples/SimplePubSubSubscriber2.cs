#region

using StackExchange.Redis;

#endregion

namespace RedisExample.PubSubExamples;

public class SimplePubSubSubscriber2(RedisService redisService, ILogger<SimplePubSubSubscriber2> logger)
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

        // Background service çalışmaya devam etsin
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void ProcessMessage(string message)
    {
        // İş mantığı burada
        logger.LogInformation("⚙️ Mesaj işleniyor: {Message}", message);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // Abonelikten çık
        await _subscriber.UnsubscribeAsync(new RedisChannel("notifications", RedisChannel.PatternMode.Literal));
        logger.LogWarning("⚠️ 'notifications' kanalından abonelik iptal edildi");

        await base.StopAsync(cancellationToken);
    }
}