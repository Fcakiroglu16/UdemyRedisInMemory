#region

using RedisExample;
using RedisExample.Endpoints.PubSub;
using RedisExample.Endpoints.PubSub.EventDrivenArchitecture;
using RedisExample.Endpoints.PubSub.Publishers;
using RedisExample.Endpoints.PubSub.Subscribers;
using RedisExample.Endpoints.RedisStream;
using RedisExample.Endpoints.RedisStream.AckExamples;
using RedisExample.Endpoints.RedisStream.AckExamples.Consumers;
using RedisExample.Endpoints.RedisStream.AckExamples.Publishers;
using RedisExample.Endpoints.RedisStream.Consumers;
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
builder.Services.AddHostedService<RedisConsumerBackgroundService>();
builder.Services.AddHostedService<RedisConsumerBackgroundService2>();
builder.Services.AddHostedService<RedisConsumerBackgroundServiceErrorExample>();
builder.Services.AddHostedService<StuckMessageRecoveryService>();

// Redis Stream ACK örnekleri (publisher + consumer, ACK açık/kapalı)
builder.Services.AddSingleton<AckEnabledStreamPublisher>();
builder.Services.AddSingleton<AckDisabledStreamPublisher>();
builder.Services.AddHostedService<AckEnabledStreamConsumer>();
builder.Services.AddHostedService<AckDisabledStreamConsumer>();
builder.Services.AddHostedService<SimplePubSubSubscriber>();
builder.Services.AddHostedService<SimplePubSubSubscriber2>();
builder.Services.AddSingleton<SimplePubSubPublisher>();

builder.Services.AddHostedService<PatternSubscriber>();


builder.Services.AddSingleton<EventBus>();
builder.Services.AddHostedService<InventoryServiceEventHandlers>();
builder.Services.AddHostedService<OrderServiceEventHandlers>();


var app = builder.Build();


app.MapGet("api/redis-check", (RedisService redisService) =>
{
    var db = redisService.GetDb(0);

    db.StringSet("key1", "value1");
    var value = db.StringGet("key1");
    return Results.Ok(value.ToString());
});

app.MapRedisStreamEndpoints();
app.MapAckStreamEndpoints();
app.MapPubSubEndpoints();


if (app.Environment.IsDevelopment()) app.MapOpenApi();


await app.RunAsync();