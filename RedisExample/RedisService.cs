#region

using StackExchange.Redis;

#endregion

namespace RedisExample;

public class RedisService
{
    private readonly ConnectionMultiplexer? _connectionMultiplexer;

    public RedisService(string redisSentinelHost, string redisSentinelPort, ILogger<RedisService> logger)
    {
        var connectionString = $"{redisSentinelHost}:{redisSentinelPort}";

        _connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);

        if (_connectionMultiplexer.IsConnected)
            logger.LogInformation("Connected to Redis Sentinel at {Host}:{Port}", redisSentinelHost, redisSentinelPort);
        else
            logger.LogError("Failed to connect to Redis Sentinel at {Host}:{Port}", redisSentinelHost,
                redisSentinelPort);
    }


    private IDatabase? Db { get; set; }


    public IDatabase GetDb(int dbIndex)
    {
        return _connectionMultiplexer!.GetDatabase(dbIndex);
    }
}