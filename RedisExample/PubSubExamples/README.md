# Redis Pub/Sub - UserCreated Event Example

## Overview
This example demonstrates a complete Pub/Sub implementation with a UserCreated event that publishes user creation events and subscribes to them, logging the results to the console.

## Components

### 1. **UserCreatedEvent** (`Events/UserCreatedEvent.cs`)
Event model containing user information:
```csharp
- UserId (Guid)
- UserName (string)
- Email (string)
- CreatedAt (DateTime)
- Role (string)
```

### 2. **SimplePubSubPublisher** (`SimplePubSubPublisher.cs`)
Publishes events to Redis channels:
- `PublishMessageAsync()` - Publishes simple string messages
- `PublishJsonMessageAsync<T>()` - Publishes any object as JSON
- `PublishUserCreatedEventAsync()` - Publishes UserCreated events

### 3. **SimplePubSubSubscriber** (`SimplePubSubSubscriber.cs`)
Background service that subscribes to Redis channels:
- Subscribes to `"notifications"` channel
- Subscribes to `"user.created"` channel
- Deserializes and logs UserCreated events to console

## How to Test

### Step 1: Start Redis
Make sure Redis is running (via Docker or local installation)

### Step 2: Run the Application
```bash
dotnet run --project RedisExample
```

The subscriber will automatically start and you'll see:
```
✅ 'notifications' kanalına abone olundu
✅ 'user.created' kanalına abone olundu
```

### Step 3: Publish a UserCreated Event
Use one of these methods:

#### Option A: HTTP POST Request
```bash
POST http://localhost:5000/api/pubsub/user-created
```

#### Option B: Using curl
```bash
curl -X POST http://localhost:5000/api/pubsub/user-created
```

#### Option C: Using PowerShell
```powershell
Invoke-WebRequest -Uri http://localhost:5000/api/pubsub/user-created -Method POST
```

### Step 4: Check Console Output
You should see something like:
```
╔════════════════════════════════════════╗
║     🎉 USER CREATED EVENT ALINDI     ║
╚════════════════════════════════════════╝
👤 User ID       : 3fa85f64-5717-4562-b3fc-2c963f66afa6
📝 User Name     : john_doe
📧 Email         : john.doe@example.com
🎭 Role          : Premium User
📅 Created At    : 2026-01-14 10:30:45
════════════════════════════════════════
```

## How It Works

1. **Publisher** serializes the UserCreatedEvent to JSON and publishes it to the `"user.created"` Redis channel
2. **Subscriber** (running as a BackgroundService) listens to the `"user.created"` channel
3. When a message arrives, it deserializes the JSON back to a UserCreatedEvent object
4. The event is then processed and logged to the console with a nice formatted output

## Key Concepts

### PatternMode.Literal
Both publisher and subscriber use `PatternMode.Literal`, meaning:
- Exact channel name matching
- Publisher sends to `"user.created"`
- Subscriber listens to `"user.created"`
- No wildcard patterns involved

### Command Execution
The publisher uses `CommandFlags.None` (default), which:
- Waits for response from Redis
- Returns the number of subscribers that received the message
- Useful for tracking message delivery

## Customization

You can create additional events by:
1. Creating a new event class in the `Events` folder
2. Adding a publish method in `SimplePubSubPublisher`
3. Adding a subscription handler in `SimplePubSubSubscriber`
4. Adding an API endpoint in `Program.cs` to trigger the event

