namespace RedisExample.Endpoints.PubSub.EventDrivenArchitecture;

public class OrderServiceEventHandlers : BackgroundService
{
    private readonly EventBus _eventBus;
    private readonly ILogger<OrderServiceEventHandlers> _logger;

    public OrderServiceEventHandlers(EventBus eventBus, ILogger<OrderServiceEventHandlers> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    [Obsolete]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Payment işlendiğinde sipariş durumunu güncelle
        await _eventBus.SubscribeAsync<PaymentProcessedEvent>(async @event =>
        {
            _logger.LogInformation(
                "💳 Ödeme alındı - OrderId: {OrderId}, Success: {Success}",
                @event.OrderId, @event.Success
            );

            if (@event.Success)
            {
                // Sipariş durumunu "Paid" yap
                await UpdateOrderStatus(@event.OrderId, "Paid");

                // Yeni event yayınla: OrderPaid
                await _eventBus.PublishAsync(new OrderPaidEvent(@event.OrderId));
            }
            else
            {
                await UpdateOrderStatus(@event.OrderId, "PaymentFailed");
            }
        });

        // Stok güncellendiğinde sipariş edilebilirliği kontrol et
        await _eventBus.SubscribeAsync<InventoryUpdatedEvent>(async @event =>
        {
            _logger.LogInformation(
                "📦 Stok güncellendi - ProductId: {ProductId}, Quantity: {Quantity}",
                @event.ProductId, @event.NewQuantity
            );

            await CheckPendingOrders(@event.ProductId);
        });

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task UpdateOrderStatus(string orderId, string status)
    {
        _logger.LogInformation("📝 Sipariş durumu güncellendi: {OrderId} -> {Status}", orderId, status);
        await Task.CompletedTask;
    }

    private async Task CheckPendingOrders(string productId)
    {
        _logger.LogInformation("🔍 Bekleyen siparişler kontrol ediliyor: {ProductId}", productId);
        await Task.CompletedTask;
    }
}

public record OrderPaidEvent(string OrderId) : IEvent
{
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
