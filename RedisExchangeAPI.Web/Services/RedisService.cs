using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedisExchangeAPI.Web.Services
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
            //   _redis = ConnectionMultiplexer.Connect("localhost"); //direk olarak localhost 6379 bağlanır

            //2.yol

            var configString = $"{_redisHost}:{_redisPort}";

            //_redis = ConnectionMultiplexer.Connect(configString);

            //3.yol sentinel
            //     _redis = ConnectionMultiplexer.Connect("localhost:26379,serviceName=mymaster,abortConnect=false");




        }

        public IDatabase GetDb(int db)
        {
            return _redis.GetDatabase(db);
        }
    }
}