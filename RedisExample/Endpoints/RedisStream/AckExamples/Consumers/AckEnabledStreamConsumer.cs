#region

using RedisExample.Endpoints.RedisStream.AckExamples.Publishers;
using StackExchange.Redis;

#endregion

namespace RedisExample.Endpoints.RedisStream.AckExamples.Consumers;

// ACK AÇIK Consumer:
// Mesajlar consumer group ile okunur (noAck: false) ve işlendikten sonra
// StreamAcknowledgeAsync (XACK) ile onaylanır.
// Onaylanan mesajlar "Pending Entries List" (PEL) içinden silinir.
// Avantaj: Consumer çökerse onaylanmamış mesajlar yeniden teslim edilebilir (güvenli).
// Dezavantaj: Her mesaj için ekstra XACK çağrısı gerekir.
public class AckEnabledStreamConsumer(RedisService redisService, ILogger<AckEnabledStreamConsumer> logger)
    : BackgroundService
{
    private const string StreamName = AckEnabledStreamPublisher.StreamName;
    private const string GroupName = "ack-enabled-group";

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
            // noAck: false (varsayılan) => mesajlar PEL'e eklenir, XACK gerekir.
            var entries = await _database!.StreamReadGroupAsync(
                StreamName,
                GroupName,
                _consumerName,
                ">", // Sadece yeni mesajlar
                10, // Bir seferde en fazla 10 mesaj al
                false // noAck: false => manuel onay (XACK) gerekir
            );

            if (entries.Length == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                continue;
            }

            foreach (var entry in entries)
            {
                var messageContent = entry.Values.FirstOrDefault(x => x.Name == "message_content").Value;
                logger.LogInformation("✅ [ACK AÇIK] İşlendi: {Content}", messageContent);

                // XACK => mesaj PEL'den silinir, "işlendi" olarak işaretlenir.
                await _database.StreamAcknowledgeAsync(StreamName, GroupName, entry.Id);
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
