using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PubSub.API.Services;
using StackExchange.Redis;

namespace PubSub.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ExampleController : ControllerBase
    {

        private readonly RedisService _redisService;

        public ExampleController(RedisService redisService)
        {
            _redisService = redisService;
        }

        [HttpGet]
        public async Task<IActionResult> Publisher()
        {



            var pubSub = _redisService.GetConnection.GetSubscriber();
            await pubSub.PublishAsync("channel1", new RedisValue("message from API"), CommandFlags.FireAndForget);
            return Ok();
        }
    }
}
