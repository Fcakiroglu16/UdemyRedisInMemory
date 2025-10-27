// Services/StuckMessageRecoveryService.cs

#region

using StackExchange.Redis;

#endregion

namespace RedisExample.Consumers;

public class StuckMessageRecoveryService(
    RedisService redisService,
    ILogger<StuckMessageRecoveryService> logger)
    : BackgroundService
{
    private const string StreamName = "my-stream-error";
    private const string GroupName = "my-consumer-group";

    // Bir mesajın "takılı kalmış" sayılması için gereken minimum süre (örn: 60 saniye)
    private const long MinIdleTimeMilliseconds = 10000;

    // Bu kurtarma servisi için benzersiz bir tüketici adı
    private readonly string _recoveryConsumerName = $"recovery-{Environment.ProcessId}";
    private readonly IDatabase _redisDb = redisService.GetDb(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Takılı kalmış mesajları kurtarma servisi başlıyor...");

        // Bu servisin çok sık çalışmasına gerek yok. 
        // 30 saniyede bir kontrol etmesi yeterlidir.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // XAUTOCLAIM komutunu çalıştır:
                // "my-stream" üzerindeki 'my-group' grubunda,
                // 'recovery-consumer' adına,
                // 60000ms'den (60sn) uzun süredir bekleyen mesajları,
                // '0-0' ID'sinden (PEL'in başından) başlayarak ara,
                // ve en fazla 10 tanesini sahiplen.
                var claimedEntries = await _redisDb.StreamAutoClaimAsync(
                    StreamName,
                    GroupName,
                    _recoveryConsumerName,
                    MinIdleTimeMilliseconds,
                    "0-0", // Bekleyenler listesinin başından itibaren ara
                    10 // Bir seferde en fazla 10 mesajı sahiplen
                );

                // 'claimedEntries.Entries' bize sahiplenilen mesajların
                // ID ve içeriklerini (NameValueEntry[]) verir.
                if (claimedEntries.ClaimedEntries != null && claimedEntries.ClaimedEntries.Length > 0)
                {
                    logger.LogWarning("{Count} adet takılı kalmış mesaj bulundu ve sahiplenildi. Yeniden işleniyor...",
                        claimedEntries.ClaimedEntries.Length);

                    foreach (var entry in claimedEntries.ClaimedEntries)
                    {
                        // 1. Mesajı işle (Tıpkı ana tüketici gibi)
                        logger.LogInformation("--> Kurtarılan Mesaj ID: {MessageId}", entry.Id);
                        var messageContent = entry.Values.FirstOrDefault(x => x.Name == "message_content").Value;
                        logger.LogInformation("    Kurtarılan İçerik: {Content}", messageContent);

                        // ... Mesaj işleme mantığı ...

                        // 2. Mesajı ONAYLA (XACK)
                        // Bu en önemli adım!
                        await _redisDb.StreamAcknowledgeAsync(StreamName, GroupName, entry.Id);
                    }
                }
                else
                {
                    logger.LogTrace("Takılı kalmış mesaj bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Mesaj kurtarma servisinde hata oluştu.");
            }

            // Her 30 saniyede bir tekrar kontrol et
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}