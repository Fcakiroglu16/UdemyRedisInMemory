#region

using StackExchange.Redis;

#endregion

namespace RedisExample.Endpoints.RedisStream.AckExamples.Publishers;

// ACK AÇIK Publisher:
// Mesaj eklenirken sunucudan onay (mesaj ID'si) BEKLENİR.
// StreamAddAsync'in döndürdüğü mesaj ID'si, yazma işleminin
// Redis tarafından onaylandığını (acknowledge) gösterir.
// Avantaj: Mesajın yazıldığından emin oluruz.
// Dezavantaj: Her mesaj için sunucu cevabı beklendiğinden throughput düşer.
public class AckEnabledStreamPublisher(RedisService redisService, ILogger<AckEnabledStreamPublisher> logger)
{
    public const string StreamName = "ack-enabled-stream";

    public async Task PublishAsync(int messageCount = 5)
    {
        var db = redisService.GetDb(1);

        for (var i = 1; i <= messageCount; i++)
        {
            var values = new[]
            {
                new NameValueEntry("message_content", $"Ack-enabled message {i}"),
                new NameValueEntry("version", "v1")
            };

            // flags belirtilmez => varsayılan davranış.
            // await, sunucu cevabını (mesaj ID'si) bekler => "ACK açık".
            var messageId = await db.StreamAddAsync(StreamName, values);

            logger.LogInformation(
                "✅ [ACK AÇIK] Mesaj yazıldı ve sunucu onayladı. ID: {MessageId}",
                messageId
            );
        }
    }
}
