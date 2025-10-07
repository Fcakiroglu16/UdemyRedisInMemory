using RedisExample;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();
builder.Services.AddSingleton<RedisService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RedisService>>();
    return new RedisService(builder.Configuration.GetSection("RedisOption")["Sentinel1:Host"]!,
        builder.Configuration.GetSection("RedisOption")["Sentinel1:Port"]!, logger);
});

var app = builder.Build();


app.MapGet("api/redis-sentinel-check", (RedisService redisService) =>
{
    var db = redisService.GetDb(0);

    db.StringSet("key1", "value1");
    var value = db.StringGet("key1");
    return Results.Ok(value.ToString());
});

if (app.Environment.IsDevelopment()) app.MapOpenApi();


await app.RunAsync();