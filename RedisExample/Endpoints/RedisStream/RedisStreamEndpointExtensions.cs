#region

using StackExchange.Redis;

#endregion

namespace RedisExample.Endpoints.RedisStream;

public static class RedisStreamEndpointExtensions
{
    public static void MapRedisStreamEndpoints(this WebApplication app)
    {
        app.MapGet("api/redis-stream-publisher", (RedisService redisService) =>
        {
            var db = redisService.GetDb(1);

            const string StreamName = "my-stream";
            Enumerable.Range(1, 20).ToList().ForEach(async void (i) =>
            {
                var values = new[]
                {
                    new NameValueEntry("message_content", $"Hello world {i}"),
                    new NameValueEntry("version", "v1")
                };


                // 1. TEMEL KULLANIM - Sadece stream adı ve değerler
                var messageId = await db.StreamAddAsync(StreamName, values);

                //// 2. messageId PARAMETRESI - Özel mesaj ID'si belirtme
                //// Senaryo: Zaman damgası veya sıralama kontrolü gerektiğinde
                //// Örnek: "1234567890123-0" formatında, ilk kısım milisaniye zaman damgası, ikinci kısım sıra numarası
                //var customMessageId = await db.StreamAddAsync(
                //    StreamName,
                //    values,
                //    messageId: "1234567890123-0"); // Özel ID, "*" otomatik ID için kullanılır

                //// 3. maxLength PARAMETRESI - Stream'in maksimum uzunluğunu sınırlama
                //// Senaryo: Bellek kullanımını kontrol etmek, eski mesajları otomatik silmek
                //// Örnek: Log sistemlerinde son 10000 mesajı tutmak
                //var messageId2 = await db.StreamAddAsync(
                //    StreamName,
                //    values,
                //    maxLength: 10000); // Stream en fazla 10000 mesaj içerir, eskiler silinir

                //// 4. useApproximateMaxLength PARAMETRESI - Yaklaşık maksimum uzunluk kullanımı
                //// Senaryo: Performans optimizasyonu, tam sayıya uymak yerine yaklaşık değer
                //// Örnek: Yüksek throughput sistemlerde performans için
                //var messageId3 = await db.StreamAddAsync(
                //    StreamName,
                //    values,
                //    maxLength: 10000,
                //    useApproximateMaxLength: true); // ~ operatörü kullanır, daha hızlı ama tam değil

                //// 5. FIELD-VALUE ÇİFTİ OLARAK - Tek bir alan-değer çifti gönderme
                //// Senaryo: Basit, tek alanlı mesajlar için
                //var messageId4 = await db.StreamAddAsync(
                //    StreamName,
                //    "field_name",
                //    "field_value");

                //// 6. FIELD-VALUE + messageId - Tek alan + özel ID
                //// Senaryo: Basit mesaj ama ID kontrolü gerekli
                //var messageId5 = await db.StreamAddAsync(
                //    StreamName,
                //    "sensor_reading",
                //    "25.5°C",
                //    messageId: "*"); // "*" Redis'in otomatik ID oluşturmasını sağlar

                //// 7. FIELD-VALUE + maxLength - Tek alan + uzunluk sınırı
                //// Senaryo: IoT sensör verileri gibi basit ama sınırlı tutulması gereken veriler
                //var messageId6 = await db.StreamAddAsync(
                //    StreamName,
                //    "temperature",
                //    "23.4",
                //    maxLength: 1000);

                //// 8. TÜM PARAMETRELER - Maksimum kontrol
                //// Senaryo: Kritik sistemlerde tam kontrol gerektiğinde
                //var messageId7 = await db.StreamAddAsync(
                //    key: StreamName,                    // Stream anahtarı
                //    streamField: "event_data",          // Tek alan adı
                //    streamValue: "critical_event",      // Tek alan değeri
                //    messageId: "*",                     // Otomatik ID
                //    maxLength: 5000,                    // Maksimum 5000 mesaj
                //    useApproximateMaxLength: true);     // Performans için yaklaşık değer
            });
        });

        app.MapGet("api/redis-stream-publisher-error", (RedisService redisService) =>
        {
            var db = redisService.GetDb(1);

            const string StreamName = "my-stream-error";
            Enumerable.Range(1, 10).ToList().ForEach(async void (i) =>
            {
                var values = new[]
                {
                    new NameValueEntry("message_content", $"Hello world {i}"),
                    new NameValueEntry("version", "v1")
                };

                var messageId = await db.StreamAddAsync(StreamName, values);
            });
        });
    }
}
