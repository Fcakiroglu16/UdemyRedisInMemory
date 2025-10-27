#region

using RedisExample;
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
var app = builder.Build();


app.MapGet("api/redis-check", (RedisService redisService) =>
{
    var db = redisService.GetDb(0);

    db.StringSet("key1", "value1");
    var value = db.StringGet("key1");
    return Results.Ok(value.ToString());
});

const string StreamName = "my-stream";
app.MapGet("api/redis-stream-publisher", (RedisService redisService) =>
{
    var db = redisService.GetDb(1);


    Enumerable.Range(1, 100).ToList().ForEach(async void (i) =>
    {
        var values = new[]
        {
            new NameValueEntry("message_content", $"Hello world {i}"),
            new NameValueEntry("version", "v1")
        };


        // StreamAddAsync, XADD komutunu çalıştırır.
        // '*' ID'nin Redis tarafından otomatik oluşturulmasını sağlar.
        var messageId = await db.StreamAddAsync(StreamName, values);
    });
});


if (app.Environment.IsDevelopment()) app.MapOpenApi();


await app.RunAsync();