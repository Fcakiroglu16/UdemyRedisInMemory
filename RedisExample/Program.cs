#region

using Microsoft.AspNetCore.Mvc;
using RedisExample;
using RedisExample.Consumers;
using RedisExample.PubSubExamples;
using RedisExample.PubSubExamples.EventDrivenArchitecture;
using Scalar.AspNetCore;
using StackExchange.Redis;

#endregion

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();
builder.Services.AddSingleton<RedisService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RedisService>>();
    return new RedisService(builder.Configuration.GetSection("RedisOption")["Sentinel1:Host"]!,
        builder.Configuration.GetSection("RedisOption")["Sentinel1:Port"]!, logger);
});
// builder.Services.AddHostedService<RedisConsumerBackgroundService>();
// builder.Services.AddHostedService<RedisConsumerBackgroundService2>();
// builder.Services.AddHostedService<RedisConsumerBackgroundServiceErrorExample>();
// builder.Services.AddHostedService<StuckMessageRecoveryService>();
//
// builder.Services.AddHostedService<SimplePubSubSubscriber2>();

// builder.Services.AddHostedService<InventoryServiceEventHandlers>();
// builder.Services.AddHostedService<OrderServiceEventHandlers>();
// builder.Services.AddHostedService<PatternSubscriber>();
// builder.Services.AddSingleton<EventBus>();


builder.Services.AddSingleton<SimplePubSubPublisher>();
builder.Services.AddHostedService<SimplePubSubSubscriber>();

var app = builder.Build();


app.MapGet("api/redis-check", (RedisService redisService) =>
{
    var db = redisService.GetDb(0);

    db.StringSet("key1", "value1");
    var value = db.StringGet("key1");
    return Results.Ok(value.ToString());
});

//
// app.MapGet("api/redis-stream-publisher", (RedisService redisService) =>
// {
//     var db = redisService.GetDb(1);
//
//     const string StreamName = "my-stream";
//     Enumerable.Range(1, 20).ToList().ForEach(async void (i) =>
//     {
//         var values = new[]
//         {
//             new NameValueEntry("message_content", $"Hello world {i}"),
//             new NameValueEntry("version", "v1")
//         };
//
//
//         // 1. TEMEL KULLANIM - Sadece stream adı ve değerler
//         var messageId = await db.StreamAddAsync(StreamName, values);
//
//         //// 2. messageId PARAMETRESI - Özel mesaj ID'si belirtme
//         //// Senaryo: Zaman damgası veya sıralama kontrolü gerektiğinde
//         //// Örnek: "1234567890123-0" formatında, ilk kısım milisaniye zaman damgası, ikinci kısım sıra numarası
//         //var customMessageId = await db.StreamAddAsync(
//         //    StreamName,
//         //    values,
//         //    messageId: "1234567890123-0"); // Özel ID, "*" otomatik ID için kullanılır
//
//         //// 3. maxLength PARAMETRESI - Stream'in maksimum uzunluğunu sınırlama
//         //// Senaryo: Bellek kullanımını kontrol etmek, eski mesajları otomatik silmek
//         //// Örnek: Log sistemlerinde son 10000 mesajı tutmak
//         //var messageId2 = await db.StreamAddAsync(
//         //    StreamName,
//         //    values,
//         //    maxLength: 10000); // Stream en fazla 10000 mesaj içerir, eskiler silinir
//
//         //// 4. useApproximateMaxLength PARAMETRESI - Yaklaşık maksimum uzunluk kullanımı
//         //// Senaryo: Performans optimizasyonu, tam sayıya uymak yerine yaklaşık değer
//         //// Örnek: Yüksek throughput sistemlerde performans için
//         //var messageId3 = await db.StreamAddAsync(
//         //    StreamName,
//         //    values,
//         //    maxLength: 10000,
//         //    useApproximateMaxLength: true); // ~ operatörü kullanır, daha hızlı ama tam değil
//
//         //// 5. FIELD-VALUE ÇİFTİ OLARAK - Tek bir alan-değer çifti gönderme
//         //// Senaryo: Basit, tek alanlı mesajlar için
//         //var messageId4 = await db.StreamAddAsync(
//         //    StreamName,
//         //    "field_name",
//         //    "field_value");
//
//         //// 6. FIELD-VALUE + messageId - Tek alan + özel ID
//         //// Senaryo: Basit mesaj ama ID kontrolü gerekli
//         //var messageId5 = await db.StreamAddAsync(
//         //    StreamName,
//         //    "sensor_reading",
//         //    "25.5°C",
//         //    messageId: "*"); // "*" Redis'in otomatik ID oluşturmasını sağlar
//
//         //// 7. FIELD-VALUE + maxLength - Tek alan + uzunluk sınırı
//         //// Senaryo: IoT sensör verileri gibi basit ama sınırlı tutulması gereken veriler
//         //var messageId6 = await db.StreamAddAsync(
//         //    StreamName,
//         //    "temperature",
//         //    "23.4",
//         //    maxLength: 1000);
//
//         //// 8. TÜM PARAMETRELER - Maksimum kontrol
//         //// Senaryo: Kritik sistemlerde tam kontrol gerektiğinde
//         //var messageId7 = await db.StreamAddAsync(
//         //    key: StreamName,                    // Stream anahtarı
//         //    streamField: "event_data",          // Tek alan adı
//         //    streamValue: "critical_event",      // Tek alan değeri
//         //    messageId: "*",                     // Otomatik ID
//         //    maxLength: 5000,                    // Maksimum 5000 mesaj
//         //    useApproximateMaxLength: true);     // Performans için yaklaşık değer
//     });
// });
//
// app.MapGet("api/redis-stream-publisher-error", (RedisService redisService) =>
// {
//     var db = redisService.GetDb(1);
//
//     const string StreamName = "my-stream-error";
//     Enumerable.Range(1, 10).ToList().ForEach(async void (i) =>
//     {
//         var values = new[]
//         {
//             new NameValueEntry("message_content", $"Hello world {i}"),
//             new NameValueEntry("version", "v1")
//         };
//
//         var messageId = await db.StreamAddAsync(StreamName, values);
//     });
// });




// app.MapPost("api/events/order/create", async (
//     EventBus eventBus) =>
// {
//     await eventBus.PublishAsync(new OrderCreatedEvent("abc", 100));
//     return Results.Ok(new { message = "Order created event published" });
// });
//
// app.MapPost("api/events/payment/process", async (
//     EventBus eventBus) =>
// {
//     await eventBus.PublishAsync(new PaymentProcessedEvent("abc", "abc", true));
//     return Results.Ok(new { message = "Payment processed event published" });
// });








app.MapGet("api/pubsub/publish", async ([FromServices]SimplePubSubPublisher publisher) =>
{
    // // Kullanıcı olayları
    // await publisher.PublishMessageAsync("user.login", "User123 logged in");
    // await publisher.PublishMessageAsync("user.logout", "User123 logged out");
    // await publisher.PublishMessageAsync("user.register", "NewUser456 registered");
    //
    // // Sipariş olayları
    // await publisher.PublishMessageAsync("order.product.created", "{orderId: 1001}");
    // await publisher.PublishMessageAsync("order.service.created", "{orderId: 1002}");
    //
    // // Hata logları
    // await publisher.PublishMessageAsync("logs.api.error", "500 Internal Server Error");
    // await publisher.PublishMessageAsync("logs.payment.error", "Payment gateway timeout");

    // Enumerable.Range(1, 10).ToList().ForEach(async i =>
    // {
    //     var message = $"Hello world {i}";
    //     await publisher.PublishMessageAsync("notifications", message);
    // });

    return Results.Ok(new { success = true });
});

app.MapPost("api/pubsub/user-created", async ([FromServices]SimplePubSubPublisher publisher) =>
{
    var userEvent = new RedisExample.PubSubExamples.Events.UserCreatedEvent
    {
        UserId = Guid.NewGuid(),
        UserName = "john_doe",
        Email = "john.doe@example.com",
        CreatedAt = DateTime.UtcNow,
        Role = "Premium User"
    };

    await publisher.PublishUserCreatedEventAsync(userEvent);

    return Results.Ok(new 
    { 
        success = true, 
        message = "UserCreated event published",
        userId = userEvent.UserId,
        userName = userEvent.UserName
    });
});



if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.MapScalarApiReference();

await app.RunAsync();