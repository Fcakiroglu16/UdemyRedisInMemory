using Microsoft.Extensions.Options;
using RedisExample.RedisConfiguration;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOptions<RedisOption>().BindConfiguration(nameof(RedisOption)).ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<RedisOption>(sp => sp.GetRequiredService<IOptions<RedisOption>>().Value);


builder.Services.AddOpenApi();
builder.Services.AddSingleton<RedisService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RedisService>>();

    var redisOption = sp.GetRequiredService<RedisOption>();
    return new RedisService(redisOption, logger);
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