#region

using StackExchange.Redis;

#endregion

namespace RedisExample.Endpoints.RedisStream.AckExamples.Publishers;

// ACK KAPALI Publisher (Fire-and-Forget):
// Mesaj gönderilir ancak sunucudan onay (cevap) BEKLENMEZ.
// CommandFlags.FireAndForget kullanıldığında dönen değer boştur (null),
// yazma işleminin başarısı garanti edilmez ama throughput ciddi şekilde artar.
// Avantaj: Çok yüksek hız.
// Dezavantaj: Mesajın yazıldığını doğrulayamayız (kaybolabilir).
public class AckDisabledStreamPublisher(RedisService redisService, ILogger<AckDisabledStreamPublisher> logger)
{
    public const string StreamName = "ack-disabled-stream";

    public async Task PublishAsync(int messageCount = 5)
    {
        var db = redisService.GetDb(1);

        for (var i = 1; i <= messageCount; i++)
        {
            var values = new[]
            {
                new NameValueEntry("message_content", $"Ack-disabled message {i}"),
                new NameValueEntry("version", "v1")
            };

            // CommandFlags.FireAndForget => sunucu cevabı beklenmez ("ACK kapalı").
            // Bu yüzden dönen messageId boş (null) olur.
            var messageId = await db.StreamAddAsync(StreamName, values, flags: CommandFlags.FireAndForget);

            logger.LogInformation(
                "🚀 [ACK KAPALI] Mesaj fire-and-forget gönderildi. Dönen ID boş mu: {IsEmpty}",
                messageId.IsNull
            );
        }
    }
}
