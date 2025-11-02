#region

using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

#endregion

namespace RedisExample.BackgroundService;

public class DistributedLockOneBackgroundService : Microsoft.Extensions.Hosting.BackgroundService
{
    public IDatabase? database { get; set; }

    public required ConnectionMultiplexer connectionMultiplexer { get; set; }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        connectionMultiplexer = ConnectionMultiplexer.Connect("localhost:6379"); // Redis sunucunuzun adresi
        database = connectionMultiplexer.GetDatabase();

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CreateLockAsync("order:123", async () =>
        {
            Console.WriteLine("order:123 for one background-service");

            // Lock acquired, perform your operations here
            await Task.Delay(30000, stoppingToken); // Simulate some work
        });
    }

    private RedLockFactory GetLockFactory()
    {
        var multiplexers = new List<RedLockMultiplexer>
        {
            connectionMultiplexer
        };
        return RedLockFactory.Create(multiplexers);
    }

    public async Task CreateLockAsync(string resource, Func<Task> lockFunction)
    {
        var expiry = TimeSpan.FromSeconds(30);
        var wait = TimeSpan.FromSeconds(10);
        var retry = TimeSpan.FromSeconds(3);


        while (true)
        {
            await using var redLock = await GetLockFactory().CreateLockAsync(resource, expiry, wait, retry);


            if (redLock.IsAcquired)
            {
                await lockFunction.Invoke();
                break;
            }

            Console.WriteLine("Lock not acquired, retrying...for one background-service");

            await Task.Delay(1000);
        }
    }
}