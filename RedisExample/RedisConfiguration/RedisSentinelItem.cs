namespace RedisExample.RedisConfiguration;

public sealed record RedisSentinelItem
{
    public required string Host { get; init; }
    public required int Port { get; init; }
}