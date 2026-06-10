using RedisExample.Endpoints.PubSub.EventDrivenArchitecture;
using RedisExample.Endpoints.PubSub.Publishers;

namespace RedisExample.Endpoints.PubSub;

public static class PubSubEndpointExtensions
{
    public static void MapPubSubEndpoints(this WebApplication app)
    {
        app.MapGet("api/pubsub/publish", async (SimplePubSubPublisher publisher) =>
        {
            // Kullanıcı olayları
            await publisher.PublishMessageAsync("user.login", "User123 logged in");
            await publisher.PublishMessageAsync("user.logout", "User123 logged out");
            await publisher.PublishMessageAsync("user.register", "NewUser456 registered");

            // Sipariş olayları
            await publisher.PublishMessageAsync("order.product.created", "{orderId: 1001}");
            await publisher.PublishMessageAsync("order.service.created", "{orderId: 1002}");

            // Hata logları
            await publisher.PublishMessageAsync("logs.api.error", "500 Internal Server Error");
            await publisher.PublishMessageAsync("logs.payment.error", "Payment gateway timeout");

            //Enumerable.Range(1, 10).ToList().ForEach(async i =>
            //{
            //    var message = $"Hello world {i}";
            //    await publisher.PublishMessageAsync("notifications", message);
            //});

            return Results.Ok(new { success = true });
        });

        app.MapPost("api/events/order/create", async (
            EventBus eventBus) =>
        {
            await eventBus.PublishAsync(new OrderCreatedEvent("abc", 100));
            return Results.Ok(new { message = "Order created event published" });
        });

        app.MapPost("api/events/payment/process", async (
            EventBus eventBus) =>
        {
            await eventBus.PublishAsync(new PaymentProcessedEvent("abc", "abc", true));
            return Results.Ok(new { message = "Payment processed event published" });
        });
    }
}
