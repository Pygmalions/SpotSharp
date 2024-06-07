using System.CommandLine;
using SpotSharp.Modules;

namespace SpotSharp.Console.Commands;

internal class ConnectCommand() : BaseCommand("", "Spot Console - Commandline Instrument for SpotSharp", isRoot: true)
{
    public SpotRobot Robot
    {
        get => Blackboard.Require<SpotRobot>();
        set => Blackboard.Set(value);
    }
    
    protected override void SetupCommand(Command command)
    {
        var argumentIp = new Argument<string>("ip", "IP address of Spot.");
        var argumentClient = new Argument<string>("client", "Client name for Spot.");
        var argumentUsername = new Argument<string>("username", "Username for Spot.");
        var argumentPassword = new Argument<string>("password", "Username for Spot.");
        command.AddArgument(argumentIp);
        command.AddArgument(argumentClient);
        command.AddArgument(argumentUsername);
        command.AddArgument(argumentPassword);
        command.SetHandler(async (ip, client, username, password) =>
        {
            Robot = SpotRobot.Connect(new Uri($"https://{ip}:443"), client);
            await Robot.Authenticate(username, password);
            Output($"Successfully authenticated. Connection to {ip} is established.");
        }, argumentIp, argumentClient, argumentUsername, argumentPassword); 
    }
}