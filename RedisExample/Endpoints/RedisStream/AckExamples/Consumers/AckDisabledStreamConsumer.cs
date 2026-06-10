#region

using RedisExample.Endpoints.RedisStream.AckExamples.Publishers;
using StackExchange.Redis;

#endregion

namespace RedisExample.Endpoints.RedisStream.AckExamples.Consumers;

// ACK KAPALI Consumer:
// Mesajlar consumer group ile okunur ancak noAck: true kullanılır.
// noAck: true => Redis mesajları otomatik onaylar, PEL'e EKLENMEZ.
// Bu yüzden XACK çağrısına gerek yoktur.
// Avantaj: Daha az ağ trafiği ve daha hızlı tüketim.
// Dezavantaj: Consumer mesajı işlerken çökerse mesaj kaybolur (yeniden teslim edilemez).
public class AckDisabledStreamConsumer(RedisService redisService, ILogger<AckDisabledStreamConsumer> logger)
    : BackgroundService
{
    private const string StreamName = AckDisabledStreamPublisher.StreamName;
    private const string GroupName = "ack-disabled-group";

    // Bu tüketici (worker) için benzersiz bir isim
    private readonly string _consumerName = $"consumer-{Environment.ProcessId}";

    private IDatabase? _database;

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _database = redisService.GetDb(1);
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await EnsureConsumerGroupAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            // noAck: true => mesajlar otomatik onaylanır, PEL'e eklenmez, XACK gerekmez.
            var entries = await _database!.StreamReadGroupAsync(
                StreamName,
                GroupName,
                _consumerName,
                ">", // Sadece yeni mesajlar
                10, // Bir seferde en fazla 10 mesaj al
                true // noAck: true => otomatik onay, XACK gerekmez
            );

            if (entries.Length == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                continue;
            }

            foreach (var entry in entries)
            {
                var messageContent = entry.Values.FirstOrDefault(x => x.Name == "message_content").Value;

                // XACK YOK => mesaj zaten otomatik onaylı (PEL'de tutulmaz).
                logger.LogInformation("🚀 [ACK KAPALI] İşlendi (otomatik onaylı): {Content}", messageContent);
            }
        }
    }

    private async Task EnsureConsumerGroupAsync()
    {
        try
        {
            // '0-0' : Stream'in başından itibaren tüm mesajları oku.
            await _database!.StreamCreateConsumerGroupAsync(StreamName, GroupName, "0-0");
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // Grup zaten varsa bu hatayı alırız, bu normal bir durum.
            logger.LogInformation("Consumer group '{GroupName}' zaten mevcut.", GroupName);
        }
    }
}
