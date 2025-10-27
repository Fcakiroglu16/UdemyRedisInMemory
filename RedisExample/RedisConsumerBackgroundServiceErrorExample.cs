#region

using StackExchange.Redis;

#endregion

namespace RedisExample;

public class RedisConsumerBackgroundServiceErrorExample(
    RedisService redisService,
    ILogger<RedisConsumerBackgroundServiceErrorExample> logger)
    : BackgroundService
{
    private const string StreamName = "my-stream-error";

    private const string GroupName = "my-consumer-group";

    // Bu tüketici (worker) için benzersiz bir isim
    private readonly string _consumerName = $"consumer-{Environment.ProcessId}";

    private IDatabase? database;

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        database = redisService.GetDb(1);
        return base.StartAsync(cancellationToken);
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // StreamCreateConsumerGroupAsync, XGROUP CREATE komutunu çalıştırır.
            // '0-0' : Stream'in başından itibaren tüm mesajları oku.
            // createStream: true : Eğer stream yoksa oluştur.
            await database!.StreamCreateConsumerGroupAsync(StreamName, GroupName, "0-0");
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // Grup zaten varsa bu hatayı alırız, bu normal bir durum.
            logger.LogInformation("Consumer group '{GroupName}' zaten mevcut.", GroupName);
        }


        while (!stoppingToken.IsCancellationRequested)
        {
            // 2. Adım: Gruptan yeni mesajları oku
            // StreamReadGroupAsync, XREADGROUP komutunu çalıştırır.
            // '>' : Bu tüketiciye daha önce hiç gönderilmemiş yeni mesajları oku.
            // block: 5000 : 5 saniye boyunca yeni mesaj gelmesini bekle (CPU'yu yormaz).
            var entries = await database.StreamReadGroupAsync(
                StreamName,
                GroupName,
                _consumerName,
                ">", // Sadece yeni mesajlar
                10 // Bir seferde en fazla 10 mesaj al
            );

            if (entries.Length == 0)
                //logger.LogInformation("Yeni mesaj yok, bekleniyor...");
                continue;

            logger.LogInformation("{Count} adet yeni mesaj işleniyor...", entries.Length);

            foreach (var entry in entries)
            {
                logger.LogInformation("--> Mesaj ID: {MessageId}", entry.Id);


                var messageContent = entry.Values.FirstOrDefault(x => x.Name == "message_content").Value;
                logger.LogInformation("    İçerik: {Content}", messageContent);
                await database.StreamAcknowledgeAsync(StreamName, GroupName, entry.Id);
            }
        }
    }
}