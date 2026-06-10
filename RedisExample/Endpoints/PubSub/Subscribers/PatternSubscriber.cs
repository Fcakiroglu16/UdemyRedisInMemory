#region

using StackExchange.Redis;

#endregion

namespace RedisExample.Endpoints.PubSub.Subscribers;

public class PatternSubscriber(RedisService redisService, ILogger<PatternSubscriber> logger)
    : BackgroundService
{
    private readonly ISubscriber _subscriber = redisService.GetSubscriber();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // PATTERN 1: user.* - Tüm kullanıcı olaylarını dinle
        // Örnek: user.login, user.logout, user.register
        await _subscriber.SubscribeAsync(
            new RedisChannel("user.*", RedisChannel.PatternMode.Pattern),
            (channel, message) =>
            {
                logger.LogInformation(
                    "👤 Kullanıcı olayı - Kanal: {Channel}, Mesaj: {Message}",
                    channel, message
                );
            }
        );

        // PATTERN 2: order.*.created - Tüm sipariş oluşturma olayları
        // Örnek: order.product.created, order.service.created
        await _subscriber.SubscribeAsync(
            new RedisChannel("order.*.created", RedisChannel.PatternMode.Pattern),
            (channel, message) =>
            {
                logger.LogInformation(
                    "🛒 Sipariş oluşturuldu - Kanal: {Channel}, Mesaj: {Message}",
                    channel, message
                );
            }
        );

        // PATTERN 3: logs.*.error - Tüm hata logları
        // Örnek: logs.api.error, logs.database.error, logs.payment.error
        await _subscriber.SubscribeAsync(
            new RedisChannel("logs.*.error", RedisChannel.PatternMode.Pattern),
            (channel, message) =>
            {
                logger.LogError(
                    "❌ Hata logu - Kanal: {Channel}, Detay: {Message}",
                    channel, message
                );

                // Kritik hatalar için alert gönder
                if (channel.ToString().Contains("payment")) SendCriticalAlert(message!);
            }
        );

        // PATTERN 4: *events* - İçinde 'events' geçen tüm kanallar
        await _subscriber.SubscribeAsync(
            new RedisChannel("*events*", RedisChannel.PatternMode.Pattern),
            (channel, message) =>
            {
                logger.LogInformation(
                    "📡 Event alındı - Kanal: {Channel}, Mesaj: {Message}",
                    channel, message
                );
            }
        );

        logger.LogInformation("✅ Pattern subscriptions aktif");
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void SendCriticalAlert(string message)
    {
        logger.LogCritical("🚨 KRİTİK UYARI: Ödeme sistemi hatası - {Message}", message);
        // Email, SMS, Slack bildirimi vb.
    }
}
