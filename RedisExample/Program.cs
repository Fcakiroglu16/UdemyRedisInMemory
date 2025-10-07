using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
// Redis'e bağlan
var redis = ConnectionMultiplexer.Connect("localhost:6379"); // Redis sunucunuzun adresi
var db = redis.GetDatabase();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

#region DataStructure

app.MapGet("/", () => "ASP.NET Core ve Redis Veri Tipleri Örnekleri (Redis-CLI Komutları ile)");

// --- 1. Strings ---
// En temel veri tipidir. Metin, sayı veya binary veri saklayabilir.
app.MapGet("/string", async () =>
{
    var key = "user:1:name";

    // Değer atama
    // redis-cli: SET user:1:name "Ahmet"
    await db.StringSetAsync(key, "Ahmet");

    // Değer okuma
    // redis-cli: GET user:1:name
    var value = await db.StringGetAsync(key);

    // Sayısal değeri 1 artırma
    // redis-cli: INCR visitor_count
    var visitorCount = await db.StringIncrementAsync("visitor_count");

    return Results.Ok(new
    {
        Name = value.ToString(),
        VisitorCount = visitorCount
    });
});

// --- 2. Lists ---
// Elemanların eklenme sırasına göre tutulduğu bir koleksiyondur.
// Kuyruk (queue) veya yığın (stack) yapıları için idealdir.
app.MapGet("/list", async () =>
{
    var key = "tasks";
    // redis-cli: DEL tasks
    await db.KeyDeleteAsync(key); // Örneği her çalıştırdığımızda listeyi temizle

    // Listenin sağına (sonuna) eleman ekleme
    // redis-cli: RPUSH tasks "task1" "task2" "task3"
    await db.ListRightPushAsync(key, new RedisValue[] { "task1", "task2", "task3" });

    // Listenin soluna (başına) eleman ekleme
    // redis-cli: LPUSH tasks "task0"
    await db.ListLeftPushAsync(key, "task0");

    // Listenin solundan (başından) bir eleman çekip silme
    // redis-cli: LPOP tasks
    var firstTask = await db.ListLeftPopAsync(key);

    // Listedeki tüm elemanları getirme
    // redis-cli: LRANGE tasks 0 -1
    var allTasks = await db.ListRangeAsync(key);

    return Results.Ok(new
    {
        RemovedTask = firstTask.ToString(),
        RemainingTasks = allTasks.Select(t => t.ToString()).ToList()
    });
});

// --- 3. Sets ---
// Sırasız ve benzersiz (unique) elemanlardan oluşan bir koleksiyondur.
// Etiketleme (tagging) veya ortak elemanları bulma gibi işlemler için kullanılır.
app.MapGet("/set", async () =>
{
    var key = "article:1:tags";
    // redis-cli: DEL article:1:tags
    await db.KeyDeleteAsync(key); // Örneği temizle

    // Sete eleman ekleme
    // redis-cli: SADD article:1:tags "redis" "csharp" "dotnet"
    await db.SetAddAsync(key, new RedisValue[] { "redis", "csharp", "dotnet" });
    await db.SetAddAsync(key, "redis"); // Tekrar eklenmez çünkü benzersizdir

    // Setteki tüm elemanları getirme
    // redis-cli: SMEMBERS article:1:tags
    var allTags = await db.SetMembersAsync(key);

    // Bir elemanın sette olup olmadığını kontrol etme
    // redis-cli: SISMEMBER article:1:tags "redis"
    var hasRedisTag = await db.SetContainsAsync(key, "redis");

    // Eleman sayısını getirme
    // redis-cli: SCARD article:1:tags
    var totalTags = await db.SetLengthAsync(key);

    return Results.Ok(new
    {
        AllTags = allTags.Select(t => t.ToString()).ToList(),
        HasRedisTag = hasRedisTag,
        TotalTags = totalTags
    });
});

// --- 4. Hashes ---
// Bir anahtar altında birden çok alan-değer (field-value) çifti saklamak için kullanılır.
// Nesneleri temsil etmek için çok uygundur.
app.MapGet("/hash", async () =>
{
    var key = "user:2";
    // Hash için alan-değer çiftleri oluşturma
    // redis-cli: HSET user:2 name "Ayşe" email "ayse@example.com" age "30"
    await db.HashSetAsync(key, new HashEntry[]
    {
        new("name", "Ayşe"),
        new("email", "ayse@example.com"),
        new("age", "30")
    });

    // Belirli bir alanı getirme
    // redis-cli: HGET user:2 name
    var name = await db.HashGetAsync(key, "name");

    // Tüm alan-değer çiftlerini getirme
    // redis-cli: HGETALL user:2
    var allFields = await db.HashGetAllAsync(key);

    // Yaş alanını 1 artırma
    // redis-cli: HINCRBY user:2 age 1
    await db.HashIncrementAsync(key, "age", 1);
    var newAge = await db.HashGetAsync(key, "age");

    return Results.Ok(new
    {
        Name = name.ToString(),
        NewAge = newAge.ToString(),
        AllFields = allFields.ToDictionary(k => k.Name.ToString(), v => v.Value.ToString())
    });
});

// --- 5. Sorted Sets (ZSET) ---
// Set'e benzer ancak her elemanın bir "skor" (score) değeri vardır.
// Elemanlar bu skora göre sıralı tutulur. Liderlik tabloları (leaderboards) için mükemmeldir.
app.MapGet("/sortedset", async () =>
{
    var key = "leaderboard";
    // redis-cli: DEL leaderboard
    await db.KeyDeleteAsync(key); // Örneği temizle

    // Skoruyla birlikte eleman ekleme
    // redis-cli: ZADD leaderboard 1500 "player:1" 2100 "player:2" 1800 "player:3"
    await db.SortedSetAddAsync(key, new SortedSetEntry[]
    {
        new("player:1", 1500),
        new("player:2", 2100),
        new("player:3", 1800)
    });

    // Bir oyuncunun skorunu artırma
    // redis-cli: ZINCRBY leaderboard 150 "player:1"
    await db.SortedSetIncrementAsync(key, "player:1", 150);

    // En yüksek skora sahip ilk 3 oyuncuyu getirme
    // redis-cli: ZREVRANGE leaderboard 0 2 WITHSCORES
    var topPlayers = await db.SortedSetRangeByRankWithScoresAsync(key, 0, 2, Order.Descending);

    return Results.Ok(topPlayers.Select(p => new
    {
        Player = p.Element.ToString(),
        Score = p.Score
    }).ToList());
});

// --- 6. Bitmaps ---
// String veri tipinin üzerinde çalışan, bit seviyesinde işlem yapmayı sağlayan bir yapıdır.
// Kullanıcıların aktif/pasif durumları gibi boolean verileri çok az bellekle saklamak için kullanılır.
app.MapGet("/bitmap", async () =>
{
    // Not: Bugünün tarihi 2025-10-03
    var key = "daily_active_users:2025-10-03";

    // 5 numaralı kullanıcı (offset) bugün aktif oldu (bit'i 1 yap)
    // redis-cli: SETBIT daily_active_users:2025-10-03 5 1
    await db.StringSetBitAsync(key, 5, true);

    // 10 numaralı kullanıcı bugün aktif oldu
    // redis-cli: SETBIT daily_active_users:2025-10-03 10 1
    await db.StringSetBitAsync(key, 10, true);

    // 5 numaralı kullanıcı pasif oldu (bit'i 0 yap)
    // redis-cli: SETBIT daily_active_users:2025-10-03 5 0
    await db.StringSetBitAsync(key, 5, false);

    // 10 numaralı kullanıcının aktif olup olmadığını kontrol et
    // redis-cli: GETBIT daily_active_users:2025-10-03 10
    var isUser10Active = await db.StringGetBitAsync(key, 10);

    // 5 numaralı kullanıcının aktif olup olmadığını kontrol et
    // redis-cli: GETBIT daily_active_users:2025-10-03 5
    var isUser5Active = await db.StringGetBitAsync(key, 5);

    // Toplam aktif kullanıcı sayısı
    // redis-cli: BITCOUNT daily_active_users:2025-10-03
    var totalActiveUsers = await db.StringBitCountAsync(key);

    return Results.Ok(new
    {
        IsUser5Active = isUser5Active,
        IsUser10Active = isUser10Active,
        TotalActiveUsers = totalActiveUsers
    });
});

// --- 7. HyperLogLogs ---
// Çok büyük bir setteki benzersiz eleman sayısını yaklaşık olarak tahmin etmek için kullanılır.
// Çok az bellek kullanır ancak %100 doğru sonuç vermez.
app.MapGet("/hyperloglog", async () =>
{
    var key = "site:unique_visitors";
    // redis-cli: DEL site:unique_visitors
    await db.KeyDeleteAsync(key); // Örneği temizle

    // Farklı kullanıcı ID'lerini ekleme
    // redis-cli: PFADD site:unique_visitors "user1" "user2" "user3" "user1"
    await db.HyperLogLogAddAsync(key, new RedisValue[] { "user1", "user2", "user3", "user1" });
    // redis-cli: PFADD site:unique_visitors "user4"
    await db.HyperLogLogAddAsync(key, "user4");

    // Benzersiz eleman sayısını tahmin etme
    // redis-cli: PFCOUNT site:unique_visitors
    var estimatedUniqueCount = await db.HyperLogLogLengthAsync(key);

    return Results.Ok(new
    {
        Note = "Bu bir tahmindir, kesin sayı değildir.",
        EstimatedUniqueVisitors = estimatedUniqueCount
    });
});

// --- 8. Geospatial Indexes ---
// Coğrafi konum verilerini (enlem, boylam) saklamak ve sorgulamak için kullanılır.
// "Yakınımdaki yerler" gibi özellikler için idealdir.
app.MapGet("/geo", async () =>
{
    var key = "cities:tr";
    // redis-cli: DEL cities:tr
    await db.KeyDeleteAsync(key); // Örneği temizle

    // Şehirleri koordinatlarıyla ekleme
    // Not: redis-cli'de sıralama `longitude latitude member` şeklindedir.
    // redis-cli: GEOADD cities:tr 32.866287 39.925533 "Ankara" 28.978359 41.008240 "Istanbul" 27.142826 38.423733 "Izmir"
    await db.GeoAddAsync(key, new GeoEntry[]
    {
        new(39.925533, 32.866287, "Ankara"), // StackExchange.Redis: latitude, longitude
        new(41.008240, 28.978359, "Istanbul"),
        new(38.423733, 27.142826, "Izmir")
    });

    // Ankara'ya 400 km yarıçapındaki şehirleri bulma
    // redis-cli: GEORADIUSBYMEMBER cities:tr Ankara 400 km
    var nearbyCities = await db.GeoRadiusAsync(key, "Ankara", 400, GeoUnit.Kilometers);

    return Results.Ok(nearbyCities.Select(c => c.Member.ToString()).ToList());
});

#endregion


//AOF Modu için: redis.conf dosyasını açın ve şu satırları bulun/ekleyin:

//Code snippet

//# AOF'yi aktifleştir
//appendonly yes

//# Her saniye diske yaz (en yaygın ve dengeli seçenek)
//appendfsync everysec
//Hybrid Mod için: AOF ayarlarının üzerine ek olarak şunu ekleyin:

//Code snippet

//# AOF yeniden yazıldığında RDB formatında bir başlangıç oluştur
//aof-use-rdb-preamble yes
//    Değişikliklerden sonra Redis sunucusunu yeniden başlatmayı unutmayın.


//Gerçek Dünya Senaryosu 🌍
//Bir e-ticaret sitesinin kullanıcı profillerini her gece saat 03:00'te yedeklemek istiyorsunuz. Anlık veri kaybı (son birkaç dakikalık profil güncellemesi gibi) çok kritik değil, ancak bir çökme durumunda dünün yedeğine dönebilmek önemli. Bu endpoint, bu gece yedeklemesini manuel olarak tetiklemek için kullanılabilir.

// --- RDB: Manuel Yedekleme Tetikleme ---
app.MapPost("/rdb-backup", async () =>
{
    // 1. Adım: Redis sunucusuna erişim sağlanır.
    // ConnectionMultiplexer üzerinden mevcut sunucu endpoint'lerini alıyoruz.
    // Genellikle tek bir sunucu olduğu için FirstOrDefault() yeterlidir.
    var server = redis.GetServer(redis.GetEndPoints().FirstOrDefault());
    if (server is null)
    {
        return Results.Problem("Redis sunucusu bulunamadı.");
    }

    // 2. Adım: Arka planda yedekleme (BGSAVE) komutu gönderilir.
    // SaveType.BackgroundSave, Redis'in yeni bir process oluşturarak
    // yedeklemeyi yapmasını sağlar. Bu sayede Redis ana process'i
    // istekleri cevaplamaya devam eder ve bloke olmaz.
    // 'SAVE' komutu ise Redis'i kitler, bu yüzden tercih edilmez.
    await server.SaveAsync(SaveType.BackgroundSave);

    // 3. Adım: Başarı mesajı döndürülür.
    // Bu komut Redis'e gönderildikten sonra işlem arka planda devam eder.
    // Redis, dump.rdb dosyasını güncelleyecektir.
    return Results.Ok(new
    {
        Message = "Arka planda RDB yedekleme işlemi başlatıldı. 'dump.rdb' dosyası güncellenecek."
    });
});


//2.AOF(Append - Only File) Senaryosu Endpoint'i
//AOF, veriyi değiştiren her komutu bir dosyaya (appendonly.aof) art arda ekler. Bu sayede veri kaybı riski minimuma iner.

//    Gerçek Dünya Senaryosu 🌍
//Bir bankacılık uygulamasında para transferi işlemi yapılıyor. Bu işlem birkaç adımdan oluşur: gönderenin bakiyesini azalt, alıcının bakiyesini artır ve işlem kaydı oluştur. Bu adımların herhangi birinde sistem çökerse, hiçbir işlemin kaybolmaması gerekir. AOF burada hayat kurtarır.


// --- AOF: Kritik İşlem Senaryosu ---
app.MapPost("/aof-critical-transaction", async (string fromAccount, string toAccount, decimal amount) =>
{
    // Bu endpoint'in güvenilir çalışması için Redis'in AOF modunda olması gerekir.

    var fromKey = $"account:{fromAccount}";
    var toKey = $"account:{toAccount}";
    var transactionId = Guid.NewGuid().ToString();
    var transactionKey = $"transaction:{transactionId}";

    // Örnek başlangıç verileri (normalde zaten Redis'te olurlar)
    await db.StringSetAsync(fromKey, 1000); // Gönderenin 1000 TL'si var
    await db.StringSetAsync(toKey, 500); // Alıcının 500 TL'si var

    // 1. Adım: Gönderenin bakiyesini azalt.
    // redis-cli: DECRBY account:123 150
    await db.StringDecrementAsync(fromKey, (double)amount);

    // 2. Adım: Alıcının bakiyesini artır.
    // redis-cli: INCRBY account:456 150
    await db.StringIncrementAsync(toKey, (double)amount);

    // 3. Adım: İşlem detaylarını bir Hash'e kaydet.
    // redis-cli: HSET transaction:abc-123 from "123" to "456" amount "150"
    await db.HashSetAsync(transactionKey, new HashEntry[]
    {
        new("from", fromAccount),
        new("to", toAccount),
        new("amount", amount.ToString())
    });

    // 4. Adım: Sunucu çökerse ne olur?
    // Eğer Redis sunucusu bu noktada çöküp yeniden başlarsa,
    // AOF dosyası sayesinde bu 3 komut da sırayla tekrar çalıştırılır.
    // Veri kaybı olmaz. RDB olsaydı ve son snapshot 5 dakika önce alınmış olsaydı,
    // bu transfer işlemi tamamen kaybolurdu.

    return Results.Ok(new
    {
        Message = "Transfer işlemi başarıyla kaydedildi.",
        TransactionId = transactionId
    });
});


app.Run();