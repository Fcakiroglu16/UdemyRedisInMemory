#region

using RedisExample.Endpoints.RedisStream.AckExamples.Publishers;

#endregion

namespace RedisExample.Endpoints.RedisStream.AckExamples;

public static class AckStreamEndpointExtensions
{
    public static void MapAckStreamEndpoints(this WebApplication app)
    {
        // ACK AÇIK publisher => her mesaj için sunucu onayı (mesaj ID) beklenir.
        app.MapGet("api/redis-stream/ack-enabled/publish",
            async (AckEnabledStreamPublisher publisher) =>
            {
                await publisher.PublishAsync();
                return Results.Ok(new { mode = "ack-enabled", success = true });
            });

        // ACK KAPALI publisher => fire-and-forget, sunucu onayı beklenmez.
        app.MapGet("api/redis-stream/ack-disabled/publish",
            async (AckDisabledStreamPublisher publisher) =>
            {
                await publisher.PublishAsync();
                return Results.Ok(new { mode = "ack-disabled", success = true });
            });
    }
}
