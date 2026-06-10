#region

using StackExchange.Redis;

#endregion

namespace RedisExample.Endpoints.PubSub.Subscribers;

public class MultiChannelSubscriber : BackgroundService
{
    private readonly Dictionary<string, Action<string>> _channelHandlers;
    private readonly ILogger<MultiChannelSubscriber> _logger;
    private readonly ISubscriber _subscriber;

    public MultiChannelSubscriber(RedisService redisService, ILogger<MultiChannelSubscriber> logger)
    {
        _subscriber = redisService.GetSubscriber();
        _logger = logger;

        // Her kanal için özel handler tanımla
        _channelHandlers = new Dictionary<string, Action<string>>
        {
            { "orders", HandleOrderMessage },
            { "payments", HandlePaymentMessage },
            { "inventory", HandleInventoryMessage },
            { "notifications", HandleNotificationMessage },
            { "analytics", HandleAnalyticsMessage }
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Tüm kanallara tek seferde abone ol
        foreach (var channelName in _channelHandlers.Keys)
        {
            await _subscriber.SubscribeAsync(
                new RedisChannel(channelName, RedisChannel.PatternMode.Literal),
                (channel, message) =>
                {
                    var channelStr = channel.ToString();
                    if (_channelHandlers.TryGetValue(channelStr, out var handler)) handler(message!);
                }
            );

            _logger.LogInformation("✅ '{Channel}' kanalına abone olundu", channelName);
        }

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void HandleOrderMessage(string message)
    {
        _logger.LogInformation("🛍️ Sipariş mesajı: {Message}", message);
        // Sipariş işleme mantığı
    }

    private void HandlePaymentMessage(string message)
    {
        _logger.LogInformation("💳 Ödeme mesajı: {Message}", message);
        // Ödeme işleme mantığı
    }

    private void HandleInventoryMessage(string message)
    {
        _logger.LogInformation("📦 Envanter mesajı: {Message}", message);
        // Stok güncelleme mantığı
    }

    private void HandleNotificationMessage(string message)
    {
        _logger.LogInformation("🔔 Bildirim mesajı: {Message}", message);
        // Bildirim gönderme mantığı
    }

    private void HandleAnalyticsMessage(string message)
    {
        _logger.LogInformation("📊 Analitik mesajı: {Message}", message);
        // Metrik toplama mantığı
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // Tüm aboneliklerden çık
        await _subscriber.UnsubscribeAllAsync();
        _logger.LogWarning("⚠️ Tüm kanallardan abonelik iptal edildi");

        await base.StopAsync(cancellationToken);
    }
}
