
using System.CommandLine;
using Bosdyn.Api;

namespace SpotSharp.Test;

public static class Program
{
    public static async Task<int> Main(string[] arguments)
    {
        var commands = new RootCommand("SpotSharp Test");

        var argumentIp = new Argument<string>("ip", "IP address of Spot.");
        var argumentUsername = new Argument<string>("username", "Username for Spot.");
        var argumentPassword = new Argument<string>("password", "Username for Spot.");
        var argumentClient = new Argument<string>("client", "Client name for Spot.");
        commands.AddArgument(argumentIp);
        commands.AddArgument(argumentUsername);
        commands.AddArgument(argumentPassword);
        commands.AddArgument(argumentClient);
        commands.SetHandler(async (ip, username, password, client) =>
        {
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
        }, argumentIp, argumentUsername, argumentPassword, argumentClient);
        
        return await commands.InvokeAsync(arguments);
    }
}