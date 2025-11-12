using StackExchange.Redis;

namespace RedisExample;

/// <summary>
/// Redis Veri Tipleri Referans Sınıfı
/// Bu sınıf, Redis'in tüm veri tiplerini hem Redis CLI komutları hem de
/// StackExchange.Redis C# komutları ile açıklamaktadır.
/// </summary>
public class RedisDataTypesReference
{
    private readonly IDatabase _db;

    public RedisDataTypesReference(IDatabase db)
    {
        _db = db;
    }

    #region 1. STRING - En Temel Veri Tipi

    /// <summary>
    /// STRING: Metin, sayı veya binary veri saklayabilir (maksimum 512 MB)
    /// Kullanım Alanları: Cache, session, counter, distributed lock
    /// </summary>
    public async Task StringOperations()
    {
        // ========== SET: Değer Atama ==========
        // Redis CLI: SET mykey "Hello World"
        await _db.StringSetAsync("mykey", "Hello World");

        // Redis CLI: SET mykey "Hello" EX 60 (60 saniye TTL ile)
        await _db.StringSetAsync("mykey", "Hello", TimeSpan.FromSeconds(60));

        // Redis CLI: SET mykey "value" NX (sadece key yoksa set et)
        await _db.StringSetAsync("mykey", "value", when: When.NotExists);

        // Redis CLI: SET mykey "value" XX (sadece key varsa set et)
        await _db.StringSetAsync("mykey", "value", when: When.Exists);

        // ========== GET: Değer Okuma ==========
        // Redis CLI: GET mykey
        var value = await _db.StringGetAsync("mykey");

        // ========== GETSET: Eski değeri al, yeni değer set et ==========
        // Redis CLI: GETSET mykey "new value"
        var oldValue = await _db.StringGetSetAsync("mykey", "new value");

        // ========== MSET: Birden fazla key-value set etme ==========
        // Redis CLI: MSET key1 "value1" key2 "value2" key3 "value3"
        await _db.StringSetAsync(new[]
        {
            new KeyValuePair<RedisKey, RedisValue>("key1", "value1"),
            new KeyValuePair<RedisKey, RedisValue>("key2", "value2"),
            new KeyValuePair<RedisKey, RedisValue>("key3", "value3")
        });

        // ========== MGET: Birden fazla key'in değerini alma ==========
        // Redis CLI: MGET key1 key2 key3
        var values = await _db.StringGetAsync(new RedisKey[] { "key1", "key2", "key3" });

        // ========== INCR: Sayısal değeri 1 artırma ==========
        // Redis CLI: INCR counter
        var newCount = await _db.StringIncrementAsync("counter");

        // ========== INCRBY: Sayısal değeri belirtilen miktar artırma ==========
        // Redis CLI: INCRBY counter 5
        await _db.StringIncrementAsync("counter", 5);

        // ========== INCRBYFLOAT: Float değeri artırma ==========
        // Redis CLI: INCRBYFLOAT price 2.5
        await _db.StringIncrementAsync("price", 2.5);

        // ========== DECR: Sayısal değeri 1 azaltma ==========
        // Redis CLI: DECR counter
        await _db.StringDecrementAsync("counter");

        // ========== DECRBY: Sayısal değeri belirtilen miktar azaltma ==========
        // Redis CLI: DECRBY counter 3
        await _db.StringDecrementAsync("counter", 3);

        // ========== APPEND: String sonuna ekleme ==========
        // Redis CLI: APPEND mykey " - appended"
        await _db.StringAppendAsync("mykey", " - appended");

        // ========== STRLEN: String uzunluğunu alma ==========
        // Redis CLI: STRLEN mykey
        var length = await _db.StringLengthAsync("mykey");

        // ========== GETRANGE: String'in bir bölümünü alma ==========
        // Redis CLI: GETRANGE mykey 0 4 (0'dan 4'e kadar karakterler)
        var substring = await _db.StringGetRangeAsync("mykey", 0, 4);

        // ========== SETRANGE: String'in belirli bir yerini değiştirme ==========
        // Redis CLI: SETRANGE mykey 6 "Redis"
        await _db.StringSetRangeAsync("mykey", 6, "Redis");
    }

    #endregion

    #region 2. LIST - Sıralı Koleksiyon

    /// <summary>
    /// LIST: LinkedList yapısında, sıralı elemanlar tutar
    /// Kullanım Alanları: Queue, Stack, Timeline, Activity Feed
    /// </summary>
    public async Task ListOperations()
    {
        var listKey = "mylist";

        // ========== LPUSH: Listenin başına eleman ekleme ==========
        // Redis CLI: LPUSH mylist "item1" "item2" "item3"
        await _db.ListLeftPushAsync(listKey, new RedisValue[] { "item1", "item2", "item3" });

        // Redis CLI: LPUSH mylist "single_item"
        await _db.ListLeftPushAsync(listKey, "single_item");

        // ========== RPUSH: Listenin sonuna eleman ekleme ==========
        // Redis CLI: RPUSH mylist "last_item"
        await _db.ListRightPushAsync(listKey, "last_item");

        // Redis CLI: RPUSH mylist "a" "b" "c"
        await _db.ListRightPushAsync(listKey, new RedisValue[] { "a", "b", "c" });

        // ========== LPOP: Listenin başından eleman çıkarma ==========
        // Redis CLI: LPOP mylist
        var firstItem = await _db.ListLeftPopAsync(listKey);

        // ========== RPOP: Listenin sonundan eleman çıkarma ==========
        // Redis CLI: RPOP mylist
        var lastItem = await _db.ListRightPopAsync(listKey);

        // ========== LRANGE: Belirli aralıktaki elemanları getirme ==========
        // Redis CLI: LRANGE mylist 0 -1 (tüm elemanlar)
        var allItems = await _db.ListRangeAsync(listKey, 0, -1);

        // Redis CLI: LRANGE mylist 0 9 (ilk 10 eleman)
        var first10 = await _db.ListRangeAsync(listKey, 0, 9);

        // ========== LLEN: Liste uzunluğunu alma ==========
        // Redis CLI: LLEN mylist
        var length = await _db.ListLengthAsync(listKey);

        // ========== LINDEX: Belirli indexteki elemanı getirme ==========
        // Redis CLI: LINDEX mylist 2
        var itemAtIndex = await _db.ListGetByIndexAsync(listKey, 2);

        // ========== LSET: Belirli indexteki elemanı değiştirme ==========
        // Redis CLI: LSET mylist 2 "new_value"
        await _db.ListSetByIndexAsync(listKey, 2, "new_value");

        // ========== LINSERT: Belirli bir elemanın önüne/arkasına ekleme ==========
        // Redis CLI: LINSERT mylist BEFORE "pivot" "new_value"
        await _db.ListInsertBeforeAsync(listKey, "pivot", "new_value");

        // Redis CLI: LINSERT mylist AFTER "pivot" "new_value"
        await _db.ListInsertAfterAsync(listKey, "pivot", "new_value");

        // ========== LREM: Belirli bir değerdeki elemanları silme ==========
        // Redis CLI: LREM mylist 0 "value_to_remove" (tüm "value_to_remove" elemanlarını sil)
        await _db.ListRemoveAsync(listKey, "value_to_remove", 0);

        // Redis CLI: LREM mylist 2 "value" (baştan 2 tane "value" sil)
        await _db.ListRemoveAsync(listKey, "value", 2);

        // ========== LTRIM: Listeyi belirli aralıkla sınırlandırma ==========
        // Redis CLI: LTRIM mylist 0 99 (sadece ilk 100 elemanı tut)
        await _db.ListTrimAsync(listKey, 0, 99);

        // ========== RPOPLPUSH: Bir listeden pop edip başka listeye push ==========
        // Redis CLI: RPOPLPUSH source_list dest_list
        await _db.ListRightPopLeftPushAsync("source_list", "dest_list");

        // ========== BLPOP: Blocking left pop (liste boşsa bekler) ==========
        // Redis CLI: BLPOP mylist 5 (5 saniye bekle)
        // Not: StackExchange.Redis'te blocking operasyonlar direkt desteklenmez
        // Alternatif olarak polling yapılabilir veya Redis Streams kullanılabilir
    }

    #endregion

    #region 3. SET - Benzersiz Elemanlar Koleksiyonu

    /// <summary>
    /// SET: Sırasız, benzersiz elemanlar koleksiyonu
    /// Kullanım Alanları: Tags, Unique Visitors, Relationships, Filtering
    /// </summary>
    public async Task SetOperations()
    {
        var setKey = "myset";

        // ========== SADD: Set'e eleman ekleme ==========
        // Redis CLI: SADD myset "member1" "member2" "member3"
        await _db.SetAddAsync(setKey, new RedisValue[] { "member1", "member2", "member3" });

        // Redis CLI: SADD myset "single_member"
        var added = await _db.SetAddAsync(setKey, "single_member");

        // ========== SMEMBERS: Tüm elemanları getirme ==========
        // Redis CLI: SMEMBERS myset
        var allMembers = await _db.SetMembersAsync(setKey);

        // ========== SISMEMBER: Elemanın varlığını kontrol etme ==========
        // Redis CLI: SISMEMBER myset "member1"
        var exists = await _db.SetContainsAsync(setKey, "member1");

        // ========== SCARD: Set'teki eleman sayısı ==========
        // Redis CLI: SCARD myset
        var count = await _db.SetLengthAsync(setKey);

        // ========== SREM: Set'ten eleman silme ==========
        // Redis CLI: SREM myset "member1" "member2"
        await _db.SetRemoveAsync(setKey, new RedisValue[] { "member1", "member2" });

        // Redis CLI: SREM myset "single_member"
        var removed = await _db.SetRemoveAsync(setKey, "single_member");

        // ========== SPOP: Rastgele eleman çıkarma ==========
        // Redis CLI: SPOP myset
        var randomMember = await _db.SetPopAsync(setKey);

        // Redis CLI: SPOP myset 3 (3 rastgele eleman çıkar)
        var randomMembers = await _db.SetPopAsync(setKey, 3);

        // ========== SRANDMEMBER: Rastgele eleman getirme (silmeden) ==========
        // Redis CLI: SRANDMEMBER myset
        var randomItem = await _db.SetRandomMemberAsync(setKey);

        // Redis CLI: SRANDMEMBER myset 2 (2 rastgele eleman)
        var randomItems = await _db.SetRandomMembersAsync(setKey, 2);

        // ========== SMOVE: Bir set'ten diğerine eleman taşıma ==========
        // Redis CLI: SMOVE source_set dest_set "member"
        await _db.SetMoveAsync("source_set", "dest_set", "member");

        // ========== SINTER: İki veya daha fazla set'in kesişimini bulma ==========
        // Redis CLI: SINTER set1 set2 set3
        var intersection = await _db.SetCombineAsync(SetOperation.Intersect,
            new RedisKey[] { "set1", "set2", "set3" });

        // ========== SINTERSTORE: Kesişimi yeni bir set'e kaydetme ==========
        // Redis CLI: SINTERSTORE result_set set1 set2
        await _db.SetCombineAndStoreAsync(SetOperation.Intersect, "result_set",
            new RedisKey[] { "set1", "set2" });

        // ========== SUNION: İki veya daha fazla set'in birleşimini bulma ==========
        // Redis CLI: SUNION set1 set2 set3
        var union = await _db.SetCombineAsync(SetOperation.Union,
            new RedisKey[] { "set1", "set2", "set3" });

        // ========== SUNIONSTORE: Birleşimi yeni bir set'e kaydetme ==========
        // Redis CLI: SUNIONSTORE result_set set1 set2
        await _db.SetCombineAndStoreAsync(SetOperation.Union, "result_set",
            new RedisKey[] { "set1", "set2" });

        // ========== SDIFF: Set farkını bulma (set1'de olup set2'de olmayanlar) ==========
        // Redis CLI: SDIFF set1 set2
        var difference = await _db.SetCombineAsync(SetOperation.Difference,
            new RedisKey[] { "set1", "set2" });

        // ========== SDIFFSTORE: Farkı yeni bir set'e kaydetme ==========
        // Redis CLI: SDIFFSTORE result_set set1 set2
        await _db.SetCombineAndStoreAsync(SetOperation.Difference, "result_set",
            new RedisKey[] { "set1", "set2" });

        // ========== SSCAN: Set elemanlarını cursor ile tarama ==========
        // Redis CLI: SSCAN myset 0 MATCH "pattern*" COUNT 100
        // StackExchange.Redis'te SetScan kullanılır (IEnumerable döner)
        await foreach (var member in _db.SetScanAsync(setKey, "pattern*", 100))
        {
            // Her eleman için işlem
        }
    }

    #endregion

    #region 4. HASH - Field-Value Pairs

    /// <summary>
    /// HASH: Bir key altında field-value çiftleri saklar
    /// Kullanım Alanları: Object Storage, User Profiles, Settings
    /// </summary>
    public async Task HashOperations()
    {
        var hashKey = "myhash";

        // ========== HSET: Hash field set etme ==========
        // Redis CLI: HSET myhash field1 "value1"
        await _db.HashSetAsync(hashKey, "field1", "value1");

        // Redis CLI: HSET myhash field1 "value1" field2 "value2" field3 "value3"
        await _db.HashSetAsync(hashKey, new HashEntry[]
        {
            new("field1", "value1"),
            new("field2", "value2"),
            new("field3", "value3")
        });

        // ========== HSETNX: Sadece field yoksa set et ==========
        // Redis CLI: HSETNX myhash field1 "value"
        var wasSet = await _db.HashSetAsync(hashKey, "field1", "value", When.NotExists);

        // ========== HGET: Hash field değerini alma ==========
        // Redis CLI: HGET myhash field1
        var value = await _db.HashGetAsync(hashKey, "field1");

        // ========== HMGET: Birden fazla field değerini alma ==========
        // Redis CLI: HMGET myhash field1 field2 field3
        var values = await _db.HashGetAsync(hashKey,
            new RedisValue[] { "field1", "field2", "field3" });

        // ========== HGETALL: Tüm field-value çiftlerini alma ==========
        // Redis CLI: HGETALL myhash
        var allEntries = await _db.HashGetAllAsync(hashKey);

        // ========== HEXISTS: Field'ın varlığını kontrol etme ==========
        // Redis CLI: HEXISTS myhash field1
        var exists = await _db.HashExistsAsync(hashKey, "field1");

        // ========== HDEL: Field silme ==========
        // Redis CLI: HDEL myhash field1 field2
        await _db.HashDeleteAsync(hashKey, new RedisValue[] { "field1", "field2" });

        // Redis CLI: HDEL myhash field1
        var deleted = await _db.HashDeleteAsync(hashKey, "field1");

        // ========== HLEN: Hash'teki field sayısı ==========
        // Redis CLI: HLEN myhash
        var fieldCount = await _db.HashLengthAsync(hashKey);

        // ========== HKEYS: Tüm field isimlerini getirme ==========
        // Redis CLI: HKEYS myhash
        var keys = await _db.HashKeysAsync(hashKey);

        // ========== HVALS: Tüm değerleri getirme ==========
        // Redis CLI: HVALS myhash
        var hashValues = await _db.HashValuesAsync(hashKey);

        // ========== HINCRBY: Integer field değerini artırma ==========
        // Redis CLI: HINCRBY myhash counter 5
        var newValue = await _db.HashIncrementAsync(hashKey, "counter", 5);

        // ========== HINCRBYFLOAT: Float field değerini artırma ==========
        // Redis CLI: HINCRBYFLOAT myhash price 2.5
        var newPrice = await _db.HashIncrementAsync(hashKey, "price", 2.5);

        // ========== HSCAN: Hash field'larını cursor ile tarama ==========
        // Redis CLI: HSCAN myhash 0 MATCH "pattern*" COUNT 100
        await foreach (var entry in _db.HashScanAsync(hashKey, "pattern*", 100))
        {
            // Her field-value çifti için işlem
        }

        // ========== HSTRLEN: Field değerinin string uzunluğu ==========
        // Redis CLI: HSTRLEN myhash field1
        var stringLength = await _db.HashStringLengthAsync(hashKey, "field1");
    }

    #endregion

    #region 5. SORTED SET (ZSET) - Skorlu Sıralı Koleksiyon

    /// <summary>
    /// SORTED SET: Her elemanın bir skoru olan, skora göre sıralı benzersiz koleksiyon
    /// Kullanım Alanları: Leaderboards, Priority Queue, Time-Series, Range Queries
    /// </summary>
    public async Task SortedSetOperations()
    {
        var zsetKey = "myzset";

        // ========== ZADD: Sorted set'e skorlu eleman ekleme ==========
        // Redis CLI: ZADD myzset 10 "member1" 20 "member2" 30 "member3"
        await _db.SortedSetAddAsync(zsetKey, new SortedSetEntry[]
        {
            new("member1", 10),
            new("member2", 20),
            new("member3", 30)
        });

        // Redis CLI: ZADD myzset 15 "single_member"
        var added = await _db.SortedSetAddAsync(zsetKey, "single_member", 15);

        // ========== ZSCORE: Elemanın skorunu getirme ==========
        // Redis CLI: ZSCORE myzset member1
        var score = await _db.SortedSetScoreAsync(zsetKey, "member1");

        // ========== ZCARD: Sorted set'teki eleman sayısı ==========
        // Redis CLI: ZCARD myzset
        var count = await _db.SortedSetLengthAsync(zsetKey);

        // ========== ZCOUNT: Belirli skor aralığındaki eleman sayısı ==========
        // Redis CLI: ZCOUNT myzset 10 30
        var rangeCount = await _db.SortedSetLengthAsync(zsetKey, 10, 30);

        // ========== ZINCRBY: Elemanın skorunu artırma ==========
        // Redis CLI: ZINCRBY myzset 5 "member1"
        var newScore = await _db.SortedSetIncrementAsync(zsetKey, "member1", 5);

        // ========== ZRANGE: İndex aralığına göre elemanları getirme (düşükten yükseğe) ==========
        // Redis CLI: ZRANGE myzset 0 -1 (tüm elemanlar)
        var allMembers = await _db.SortedSetRangeByRankAsync(zsetKey, 0, -1, Order.Ascending);

        // Redis CLI: ZRANGE myzset 0 9 WITHSCORES (ilk 10 eleman skorlarıyla)
        var top10WithScores = await _db.SortedSetRangeByRankWithScoresAsync(zsetKey, 0, 9,
            Order.Ascending);

        // ========== ZREVRANGE: Ters sırada (yüksekten düşüğe) elemanları getirme ==========
        // Redis CLI: ZREVRANGE myzset 0 9 (en yüksek skorlu ilk 10)
        var topScorers = await _db.SortedSetRangeByRankAsync(zsetKey, 0, 9, Order.Descending);

        // ========== ZRANGEBYSCORE: Skor aralığına göre elemanları getirme ==========
        // Redis CLI: ZRANGEBYSCORE myzset 10 30
        var membersByScore = await _db.SortedSetRangeByScoreAsync(zsetKey, 10, 30);

        // Redis CLI: ZRANGEBYSCORE myzset -inf +inf LIMIT 0 10
        var limitedRange = await _db.SortedSetRangeByScoreAsync(zsetKey,
            double.NegativeInfinity, double.PositiveInfinity, take: 10);

        // ========== ZREVRANGEBYSCORE: Ters sırada skor aralığına göre getirme ==========
        // Redis CLI: ZREVRANGEBYSCORE myzset 30 10
        var reverseMembersByScore = await _db.SortedSetRangeByScoreAsync(zsetKey, 30, 10,
            order: Order.Descending);

        // ========== ZRANK: Elemanın sırasını (rank) bulma (0'dan başlar, düşükten yükseğe) ==========
        // Redis CLI: ZRANK myzset "member1"
        var rank = await _db.SortedSetRankAsync(zsetKey, "member1", Order.Ascending);

        // ========== ZREVRANK: Ters sıradaki rank (yüksekten düşüğe) ==========
        // Redis CLI: ZREVRANK myzset "member1"
        var reverseRank = await _db.SortedSetRankAsync(zsetKey, "member1", Order.Descending);

        // ========== ZREM: Eleman silme ==========
        // Redis CLI: ZREM myzset "member1" "member2"
        await _db.SortedSetRemoveAsync(zsetKey, new RedisValue[] { "member1", "member2" });

        // Redis CLI: ZREM myzset "single_member"
        var removed = await _db.SortedSetRemoveAsync(zsetKey, "single_member");

        // ========== ZREMRANGEBYRANK: Rank aralığına göre silme ==========
        // Redis CLI: ZREMRANGEBYRANK myzset 0 9 (ilk 10 elemanı sil)
        await _db.SortedSetRemoveRangeByRankAsync(zsetKey, 0, 9);

        // ========== ZREMRANGEBYSCORE: Skor aralığına göre silme ==========
        // Redis CLI: ZREMRANGEBYSCORE myzset 10 30
        await _db.SortedSetRemoveRangeByScoreAsync(zsetKey, 10, 30);

        // ========== ZPOPMIN: En düşük skorlu elemanı çıkarma ==========
        // Redis CLI: ZPOPMIN myzset
        var minMember = await _db.SortedSetPopAsync(zsetKey, Order.Ascending);

        // Redis CLI: ZPOPMIN myzset 3 (en düşük 3 elemanı çıkar)
        var minMembers = await _db.SortedSetPopAsync(zsetKey, 3, Order.Ascending);

        // ========== ZPOPMAX: En yüksek skorlu elemanı çıkarma ==========
        // Redis CLI: ZPOPMAX myzset
        var maxMember = await _db.SortedSetPopAsync(zsetKey, Order.Descending);

        // ========== ZUNIONSTORE: Sorted setlerin birleşimini yeni bir set'e kaydetme ==========
        // Redis CLI: ZUNIONSTORE result_zset 2 zset1 zset2 WEIGHTS 2 3
        await _db.SortedSetCombineAndStoreAsync(SetOperation.Union, "result_zset",
            new RedisKey[] { "zset1", "zset2" }, new double[] { 2, 3 });

        // ========== ZINTERSTORE: Sorted setlerin kesişimini yeni bir set'e kaydetme ==========
        // Redis CLI: ZINTERSTORE result_zset 2 zset1 zset2
        await _db.SortedSetCombineAndStoreAsync(SetOperation.Intersect, "result_zset",
            new RedisKey[] { "zset1", "zset2" });

        // ========== ZSCAN: Sorted set elemanlarını cursor ile tarama ==========
        // Redis CLI: ZSCAN myzset 0 MATCH "pattern*" COUNT 100
        await foreach (var entry in _db.SortedSetScanAsync(zsetKey, "pattern*", 100))
        {
            // Her member-score çifti için işlem
        }

        // ========== ZLEXCOUNT: Lexicographical olarak eleman sayısı ==========
        // Redis CLI: ZLEXCOUNT myzset [a [z
        // Not: Tüm elemanlar aynı skora sahip olmalı
        var lexCount = await _db.SortedSetLengthByValueAsync(zsetKey, "a", "z");

        // ========== ZRANGEBYLEX: Lexicographical sırayla elemanları getirme ==========
        // Redis CLI: ZRANGEBYLEX myzset [a [z
        var lexRange = await _db.SortedSetRangeByValueAsync(zsetKey, "a", "z");
    }

    #endregion

    #region 6. BITMAP - Bit Düzeyinde İşlemler

    /// <summary>
    /// BITMAP: String veri tipi üzerinde bit seviyesinde işlemler
    /// Kullanım Alanları: User Activity Tracking, Feature Flags, Real-time Analytics
    /// </summary>
    public async Task BitmapOperations()
    {
        var bitmapKey = "mybitmap";

        // ========== SETBIT: Belirli offset'teki bit'i set etme ==========
        // Redis CLI: SETBIT mybitmap 10 1 (10. bit'i 1 yap)
        await _db.StringSetBitAsync(bitmapKey, 10, true);

        // Redis CLI: SETBIT mybitmap 5 0 (5. bit'i 0 yap)
        await _db.StringSetBitAsync(bitmapKey, 5, false);

        // ========== GETBIT: Belirli offset'teki bit'i okuma ==========
        // Redis CLI: GETBIT mybitmap 10
        var bitValue = await _db.StringGetBitAsync(bitmapKey, 10);

        // ========== BITCOUNT: 1 olan bit sayısını sayma ==========
        // Redis CLI: BITCOUNT mybitmap
        var setBitsCount = await _db.StringBitCountAsync(bitmapKey);

        // Redis CLI: BITCOUNT mybitmap 0 10 (0-10 byte aralığında)
        var rangeCount = await _db.StringBitCountAsync(bitmapKey, 0, 10);

        // ========== BITPOS: İlk 0 veya 1 bit pozisyonunu bulma ==========
        // Redis CLI: BITPOS mybitmap 1 (ilk 1 bit'in pozisyonu)
        var firstSetBit = await _db.StringBitPositionAsync(bitmapKey, true);

        // Redis CLI: BITPOS mybitmap 0 (ilk 0 bit'in pozisyonu)
        var firstUnsetBit = await _db.StringBitPositionAsync(bitmapKey, false);

        // ========== BITOP: Bitmap'ler arası bit operasyonları ==========
        // Redis CLI: BITOP AND dest_key bitmap1 bitmap2 bitmap3
        await _db.StringBitOperationAsync(Bitwise.And, "dest_key",
            new RedisKey[] { "bitmap1", "bitmap2", "bitmap3" });

        // Redis CLI: BITOP OR dest_key bitmap1 bitmap2
        await _db.StringBitOperationAsync(Bitwise.Or, "dest_key",
            new RedisKey[] { "bitmap1", "bitmap2" });

        // Redis CLI: BITOP XOR dest_key bitmap1 bitmap2
        await _db.StringBitOperationAsync(Bitwise.Xor, "dest_key",
            new RedisKey[] { "bitmap1", "bitmap2" });

        // Redis CLI: BITOP NOT dest_key source_bitmap
        await _db.StringBitOperationAsync(Bitwise.Not, "dest_key", "source_bitmap");

        // ========== BITFIELD: Birden fazla bit operasyonunu atomik yapma ==========
        // Redis CLI: BITFIELD mybitmap SET u4 0 15 GET u4 0 INCRBY u4 0 1
        // Not: StackExchange.Redis'te direkt StringBitField yoktur, 
        // Execute komutu ile custom komut çalıştırılabilir
    }

    #endregion

    #region 7. HYPERLOGLOG - Kardinalite Tahmini

    /// <summary>
    /// HYPERLOGLOG: Büyük veri setlerinde benzersiz eleman sayısını yaklaşık hesaplama
    /// Kullanım Alanları: Unique Visitors Count, Distinct Elements Estimation
    /// Avantaj: Çok az bellek kullanır (~12KB), Dezavantaj: %0.81 hata payı
    /// </summary>
    public async Task HyperLogLogOperations()
    {
        var hllKey = "myhll";

        // ========== PFADD: HyperLogLog'a eleman ekleme ==========
        // Redis CLI: PFADD myhll "element1" "element2" "element3"
        var modified = await _db.HyperLogLogAddAsync(hllKey,
            new RedisValue[] { "element1", "element2", "element3" });

        // Redis CLI: PFADD myhll "single_element"
        await _db.HyperLogLogAddAsync(hllKey, "single_element");

        // ========== PFCOUNT: Benzersiz eleman sayısını tahmin etme ==========
        // Redis CLI: PFCOUNT myhll
        var uniqueCount = await _db.HyperLogLogLengthAsync(hllKey);

        // Redis CLI: PFCOUNT hll1 hll2 hll3 (birden fazla HLL'nin birleşimi)
        var combinedCount = await _db.HyperLogLogLengthAsync(
            new RedisKey[] { "hll1", "hll2", "hll3" });

        // ========== PFMERGE: Birden fazla HyperLogLog'u birleştirme ==========
        // Redis CLI: PFMERGE dest_hll source_hll1 source_hll2
        await _db.HyperLogLogMergeAsync("dest_hll",
            new RedisKey[] { "source_hll1", "source_hll2" });
    }

    #endregion

    #region 8. GEOSPATIAL - Coğrafi Konum Verileri

    /// <summary>
    /// GEOSPATIAL: Coğrafi koordinatları saklama ve sorgulama
    /// Kullanım Alanları: Location-based Services, Nearby Search, Distance Calculation
    /// Arka planda Sorted Set kullanır
    /// </summary>
    public async Task GeospatialOperations()
    {
        var geoKey = "mygeo";

        // ========== GEOADD: Coğrafi konum ekleme ==========
        // Redis CLI: GEOADD mygeo 13.361389 38.115556 "Palermo" 15.087269 37.502669 "Catania"
        // Not: Redis CLI'de longitude (boylam) önce gelir!
        await _db.GeoAddAsync(geoKey, new GeoEntry[]
        {
            new(38.115556, 13.361389, "Palermo"), // C#'ta latitude önce
            new(37.502669, 15.087269, "Catania")
        });

        // Redis CLI: GEOADD mygeo 12.496366 41.902782 "Rome"
        await _db.GeoAddAsync(geoKey, 41.902782, 12.496366, "Rome");

        // ========== GEODIST: İki konum arası mesafe hesaplama ==========
        // Redis CLI: GEODIST mygeo Palermo Catania km
        var distance = await _db.GeoDistanceAsync(geoKey, "Palermo", "Catania", GeoUnit.Kilometers);

        // Redis CLI: GEODIST mygeo Palermo Rome m (metre cinsinden)
        var distanceMeters = await _db.GeoDistanceAsync(geoKey, "Palermo", "Rome", GeoUnit.Meters);

        // ========== GEOPOS: Konumun koordinatlarını getirme ==========
        // Redis CLI: GEOPOS mygeo Palermo Catania
        var positions = await _db.GeoPositionAsync(geoKey, new RedisValue[] { "Palermo", "Catania" });

        // ========== GEORADIUS: Belirli koordinata yakın konumları bulma ==========
        // Redis CLI: GEORADIUS mygeo 15 37 200 km WITHDIST WITHCOORD
        var nearbyLocations = await _db.GeoRadiusAsync(geoKey, 15, 37, 200, GeoUnit.Kilometers,
            options: GeoRadiusOptions.WithDistance | GeoRadiusOptions.WithCoordinates);

        // Redis CLI: GEORADIUS mygeo 15 37 100 km COUNT 5 ASC
        var closestFive = await _db.GeoRadiusAsync(geoKey, 15, 37, 100, GeoUnit.Kilometers,
            count: 5, order: Order.Ascending);

        // ========== GEORADIUSBYMEMBER: Bir üyeye yakın konumları bulma ==========
        // Redis CLI: GEORADIUSBYMEMBER mygeo Palermo 200 km
        var nearPalermo = await _db.GeoRadiusAsync(geoKey, "Palermo", 200, GeoUnit.Kilometers);

        // Redis CLI: GEORADIUSBYMEMBER mygeo Rome 500 km WITHDIST
        var nearRome = await _db.GeoRadiusAsync(geoKey, "Rome", 500, GeoUnit.Kilometers,
            options: GeoRadiusOptions.WithDistance);

        // ========== GEOHASH: Konumun geohash değerini alma ==========
        // Redis CLI: GEOHASH mygeo Palermo Catania
        var geohashes = await _db.GeoHashAsync(geoKey, new RedisValue[] { "Palermo", "Catania" });

        // ========== GEOSEARCH: Gelişmiş coğrafi arama (Redis 6.2+) ==========
        // Redis CLI: GEOSEARCH mygeo FROMLONLAT 15 37 BYRADIUS 200 km
        // Not: StackExchange.Redis'in sürümüne göre GeoSearch desteği değişebilir
        // Alternatif olarak GeoRadius kullanılabilir
    }

    #endregion

    #region 9. STREAM - Log/Event Streaming

    /// <summary>
    /// STREAM: Append-only log yapısı, mesaj kuyruğu ve event streaming
    /// Kullanım Alanları: Event Sourcing, Message Queue, Activity Log, Chat Messages
    /// Redis 5.0+ ile gelen güçlü bir veri tipi
    /// </summary>
    public async Task StreamOperations()
    {
        var streamKey = "mystream";

        // ========== XADD: Stream'e mesaj ekleme ==========
        // Redis CLI: XADD mystream * field1 "value1" field2 "value2"
        // * işareti otomatik ID oluşturur (timestamp-sequence)
        var messageId = await _db.StreamAddAsync(streamKey, new[]
        {
            new NameValueEntry("field1", "value1"),
            new NameValueEntry("field2", "value2")
        });

        // Redis CLI: XADD mystream 1234567890-0 name "John" age "30"
        // Özel ID belirtme (normalde önerilmez)
        await _db.StreamAddAsync(streamKey, "name", "John", "1234567890-0");

        // ========== XLEN: Stream'deki mesaj sayısı ==========
        // Redis CLI: XLEN mystream
        var messageCount = await _db.StreamLengthAsync(streamKey);

        // ========== XRANGE: ID aralığına göre mesajları okuma ==========
        // Redis CLI: XRANGE mystream - + (tüm mesajlar)
        var allMessages = await _db.StreamRangeAsync(streamKey);

        // Redis CLI: XRANGE mystream 1234567890 1234567900 COUNT 10
        var rangeMessages = await _db.StreamRangeAsync(streamKey, "1234567890", "1234567900", 10);

        // ========== XREVRANGE: Ters sırada mesajları okuma ==========
        // Redis CLI: XREVRANGE mystream + - COUNT 10 (son 10 mesaj)
        var lastMessages = await _db.StreamRangeAsync(streamKey, "+", "-", 10,
            messageOrder: Order.Descending);

        // ========== XREAD: Stream'den mesaj okuma (blocking destekler) ==========
        // Redis CLI: XREAD COUNT 2 STREAMS mystream 0
        var readMessages = await _db.StreamReadAsync(streamKey, "0", 2);

        // Redis CLI: XREAD BLOCK 5000 STREAMS mystream $ (5 saniye bekle, yeni mesajları oku)
        // Not: StackExchange.Redis'te blocking için StreamPosition kullanılır

        // ========== XREADGROUP: Consumer group ile mesaj okuma ==========
        // Önce group oluşturmak gerekir: XGROUP CREATE mystream mygroup 0
        await _db.StreamCreateConsumerGroupAsync(streamKey, "mygroup", "0");

        // Redis CLI: XREADGROUP GROUP mygroup consumer1 COUNT 1 STREAMS mystream >
        var groupMessages = await _db.StreamReadGroupAsync(streamKey, "mygroup", "consumer1",
            ">", 1);

        // ========== XACK: Mesajı acknowledge etme ==========
        // Redis CLI: XACK mystream mygroup 1234567890-0
        await _db.StreamAcknowledgeAsync(streamKey, "mygroup", messageId);

        // ========== XPENDING: Bekleyen (acknowledge edilmemiş) mesajları görme ==========
        // Redis CLI: XPENDING mystream mygroup
        var pendingInfo = await _db.StreamPendingAsync(streamKey, "mygroup");

        // Redis CLI: XPENDING mystream mygroup - + 10 consumer1
        var pendingMessages = await _db.StreamPendingMessagesAsync(streamKey, "mygroup", 10,
            "consumer1");

        // ========== XCLAIM: Başka bir consumer'ın mesajını claim etme ==========
        // Redis CLI: XCLAIM mystream mygroup consumer2 3600000 1234567890-0
        var claimedMessages = await _db.StreamClaimAsync(streamKey, "mygroup", "consumer2",
            3600000, new[] { messageId });

        // ========== XDEL: Stream'den mesaj silme ==========
        // Redis CLI: XDEL mystream 1234567890-0
        await _db.StreamDeleteAsync(streamKey, new[] { messageId });

        // ========== XTRIM: Stream'i belirli uzunlukta tutma ==========
        // Redis CLI: XTRIM mystream MAXLEN 1000 (son 1000 mesajı tut)
        await _db.StreamTrimAsync(streamKey, 1000);

        // Redis CLI: XTRIM mystream MAXLEN ~ 1000 (yaklaşık 1000, daha performanslı)
        await _db.StreamTrimAsync(streamKey, 1000, useApproximateMaxLength: true);

        // ========== XINFO: Stream hakkında bilgi alma ==========
        // Redis CLI: XINFO STREAM mystream
        var streamInfo = await _db.StreamInfoAsync(streamKey);

        // Redis CLI: XINFO GROUPS mystream
        var groupsInfo = await _db.StreamGroupInfoAsync(streamKey);

        // Redis CLI: XINFO CONSUMERS mystream mygroup
        var consumersInfo = await _db.StreamConsumerInfoAsync(streamKey, "mygroup");

        // ========== XGROUP: Consumer group yönetimi ==========
        // Redis CLI: XGROUP CREATE mystream mygroup2 $ MKSTREAM
        await _db.StreamCreateConsumerGroupAsync(streamKey, "mygroup2", StreamPosition.NewMessages,
            createStream: true);

        // Redis CLI: XGROUP DESTROY mystream mygroup2
        await _db.StreamDeleteConsumerGroupAsync(streamKey, "mygroup2");

        // Redis CLI: XGROUP DELCONSUMER mystream mygroup consumer1
        await _db.StreamDeleteConsumerAsync(streamKey, "mygroup", "consumer1");
    }

    #endregion

    #region 10. GENERIC KEY OPERATIONS - Tüm Veri Tiplerinde Geçerli Komutlar

    /// <summary>
    /// GENERIC COMMANDS: Tüm veri tiplerinde kullanılabilecek key işlemleri
    /// </summary>
    public async Task GenericKeyOperations()
    {
        // ========== EXISTS: Key'in varlığını kontrol etme ==========
        // Redis CLI: EXISTS mykey
        var exists = await _db.KeyExistsAsync("mykey");

        // Redis CLI: EXISTS key1 key2 key3 (kaç tane var)
        var existsCount = await _db.KeyExistsAsync(new RedisKey[] { "key1", "key2", "key3" });

        // ========== DEL: Key silme ==========
        // Redis CLI: DEL mykey
        var deleted = await _db.KeyDeleteAsync("mykey");

        // Redis CLI: DEL key1 key2 key3
        await _db.KeyDeleteAsync(new RedisKey[] { "key1", "key2", "key3" });

        // ========== EXPIRE: Key'e TTL (Time To Live) atama ==========
        // Redis CLI: EXPIRE mykey 60 (60 saniye sonra silinecek)
        await _db.KeyExpireAsync("mykey", TimeSpan.FromSeconds(60));

        // ========== EXPIREAT: Belirli bir zamanda silinmesi için ayarlama ==========
        // Redis CLI: EXPIREAT mykey 1735689600 (Unix timestamp)
        await _db.KeyExpireAsync("mykey", DateTime.UtcNow.AddDays(1));

        // ========== PEXPIRE: Milisaniye cinsinden TTL ==========
        // Redis CLI: PEXPIRE mykey 60000 (60000ms = 60 saniye)
        await _db.KeyExpireAsync("mykey", TimeSpan.FromMilliseconds(60000));

        // ========== TTL: Kalan süreyi saniye cinsinden öğrenme ==========
        // Redis CLI: TTL mykey
        var ttl = await _db.KeyTimeToLiveAsync("mykey");

        // ========== PTTL: Kalan süreyi milisaniye cinsinden öğrenme ==========
        // Redis CLI: PTTL mykey
        // StackExchange.Redis TTL'i TimeSpan olarak döndürür

        // ========== PERSIST: TTL'i kaldırma (kalıcı yapma) ==========
        // Redis CLI: PERSIST mykey
        await _db.KeyPersistAsync("mykey");

        // ========== TYPE: Key'in veri tipini öğrenme ==========
        // Redis CLI: TYPE mykey (string, list, set, zset, hash, stream döner)
        var keyType = await _db.KeyTypeAsync("mykey");

        // ========== RENAME: Key'i yeniden adlandırma ==========
        // Redis CLI: RENAME old_key new_key
        await _db.KeyRenameAsync("old_key", "new_key");

        // ========== RENAMENX: Sadece yeni key yoksa rename et ==========
        // Redis CLI: RENAMENX old_key new_key
        var renamed = await _db.KeyRenameAsync("old_key", "new_key", When.NotExists);

 
    }

#endregion
}

