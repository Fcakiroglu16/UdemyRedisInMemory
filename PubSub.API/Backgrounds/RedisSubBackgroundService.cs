using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PubSub.API.Services;

namespace PubSub.API.Backgrounds
{

    public class RedisSubBackgroundService : BackgroundService
    {
        private readonly RedisService _redisService;
        private readonly ILogger<RedisSubBackgroundService> _logger;

        public RedisSubBackgroundService(RedisService redisService, ILogger<RedisSubBackgroundService> logger)
        {
            _redisService = redisService;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var pubSub = _redisService.GetConnection.GetSubscriber();

            pubSub.SubscribeAsync("channel1", (channel, message) =>
            {

                _logger.LogInformation($"Gelen Mesaj:{message}");
            });

            return Task.CompletedTask;

        }
    }
}
