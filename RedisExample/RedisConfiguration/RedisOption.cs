namespace RedisExample.RedisConfiguration;

public record RedisOption
{
    public required List<RedisSentinelItem> Sentinels { get; init; } = null!;
    public required string Password { get; init; }
}