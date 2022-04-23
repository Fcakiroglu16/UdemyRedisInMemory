using StackExchange.Redis;

namespace PubSub.API.Services
{
    public class RedisService
    {
        private readonly string _redisHost;

        private readonly string _redisPort;
        private ConnectionMultiplexer _redis;

        public IDatabase db { get; set; }

        public RedisService(IConfiguration configuration)
        {
            _redisHost = configuration["Redis:Host"];

            _redisPort = configuration["Redis:Port"];
        }

        public void Connect()
        {
            //1.yol
            // var configString = $"{_redisHost}:{_redisPort}";

            // _redis = ConnectionMultiplexer.Connect(configString);

            //2.yol

            ConfigurationOptions options = new ConfigurationOptions
            {
                EndPoints = { { "localhost", 6379 } }


            };
            _redis = ConnectionMultiplexer.Connect(options);

        }

        public IDatabase GetDb(int db) => _redis.GetDatabase(db);

        public ConnectionMultiplexer GetConnection => _redis;

    }
}
