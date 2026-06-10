namespace RedisExample.PubSubExamples.Events;

public class UserCreatedEvent
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Role { get; set; } = "User";
}

