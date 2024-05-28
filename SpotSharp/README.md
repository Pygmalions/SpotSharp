# Spot Sharp

This library is a third party implementation of .NET client for Spot robot of Boston Dynamics.

## How to Use

### Procedure

1. Use `Robot.Connect(SPOT_URI, CLIENT_NAME, USERNAME, PASSWORD)` to create a Robot object, 
which represents an established connection to Spot.
2. Use `GetService<SERVICE_CLIENT_TYPE>(OPTIONAL_SERVICE_AUTHORITY)` on the Robot object, to get a client instance for the specified service. 
3. (Optional) Invoke `UpdateToken()` method on the Robot object to refresh the token.

### Example

The following code example shows how to list all available services on Spot.

```csharp
var ip = "YOUR_SPOT_IP";
var client = "YOUR_CLIENT_NAME";
var username = "YOUR_USERNAME";
var password = "YOUR_PASSWORD";

var robot = await Robot.Connect(new Uri($"https://{ip}:443"),
                client, username, password);
Console.WriteLine("Connected.");
var service = robot.GetService<DirectoryService.DirectoryServiceClient>();
var response = await service.ListServiceEntriesAsync(new ListServiceEntriesRequest()
{
    Header = robot.Header
});
foreach (var entry in response.ServiceEntries)
{
    Console.WriteLine($"{entry.Name} - #{entry.Type} - @{entry.Authority}");
 }
Console.WriteLine("Robot Connected.");
```