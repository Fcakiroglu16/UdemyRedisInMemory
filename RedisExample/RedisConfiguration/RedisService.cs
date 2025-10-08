using StackExchange.Redis;

namespace RedisExample.RedisConfiguration;

public class RedisService
{
    private readonly ConnectionMultiplexer? _connectionMultiplexer;

    public RedisService(RedisOption redisOption, ILogger<RedisService> logger)
    {
        var sentinelEndpoints = string.Join(",", redisOption.Sentinels.Select(s => $"{s.Host}:{s.Port}"));
        var connectionString = $"{sentinelEndpoints},serviceName=mymaster,password={redisOption.Password}";


        _connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);

        if (_connectionMultiplexer.IsConnected)
            using (logger.BeginScope("IsConnected"))
            {
                logger.LogInformation("Connected to Redis Sentinel using endpoints: {Endpoints}", sentinelEndpoints);
            }

        else
            logger.LogError("Failed to connect to Redis Sentinel using endpoints: {Endpoints}", sentinelEndpoints);


        _connectionMultiplexer.ConnectionRestored += (sender, args) =>
        {
            using (logger.BeginScope("ConnectionRestored"))
            {
                logger.LogInformation("Redis restored");
            }
        };
        _connectionMultiplexer.ConnectionFailed += (sender, args) =>
        {
            logger.LogError("Redis connection failed: {FailureType}", args.FailureType);
        };
    }


    public IDatabase? Db { get; set; }

    private void LogRedisServerEndpoints(ILogger<RedisService> logger)
    {
        var endpoints = _connectionMultiplexer!.GetEndPoints(true);


        foreach (var endpoint in endpoints)
        {
            var server = _connectionMultiplexer.GetServer(endpoint);

            if (server.IsReplica)
                logger.LogInformation("Redis Replica pod: {Endpoint}", server.EndPoint);
        }
    }


    public IDatabase GetDb(int dbIndex)
    {
        return _connectionMultiplexer!.GetDatabase(dbIndex);
    }
}