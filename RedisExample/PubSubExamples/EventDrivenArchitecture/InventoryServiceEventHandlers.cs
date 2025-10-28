namespace RedisExample.PubSubExamples.EventDrivenArchitecture;

public class InventoryServiceEventHandlers(EventBus eventBus, ILogger<InventoryServiceEventHandlers> logger)
    : BackgroundService
{
    [Obsolete]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Sipariş oluşturulduğunda stok rezerve et
        await eventBus.SubscribeAsync<OrderCreatedEvent>(async @event =>
        {
            logger.LogInformation(
                "🛒 Yeni sipariş - OrderId: {OrderId}, Stok rezerve ediliyor",
                @event.OrderId
            );

            await ReserveInventory(@event.OrderId);
        });

        // Sipariş ödendiyse stok düş
        await eventBus.SubscribeAsync<OrderPaidEvent>(async @event =>
        {
            logger.LogInformation(
                "✅ Sipariş ödendi - OrderId: {OrderId}, Stok düşülüyor",
                @event.OrderId
            );

            await DeductInventory(@event.OrderId);

            // Stok güncelleme eventi yayınla
            await eventBus.PublishAsync(new InventoryUpdatedEvent("product123", 45));
        });

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ReserveInventory(string orderId)
    {
        logger.LogInformation("🔒 Stok rezerve edildi: {OrderId}", orderId);
        await Task.CompletedTask;
    }

    private async Task DeductInventory(string orderId)
    {
        logger.LogInformation("📉 Stok düşüldü: {OrderId}", orderId);
        await Task.CompletedTask;
    }
}