using System.Drawing;
using System.Reflection;
using Bosdyn.Api;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using SpotSharp.Console.Commands;

namespace SpotSharp.Console;

using System.CommandLine;

public static class Launcher
{
    public static void InstallCommandsFromAssembly(BaseCommand root, Assembly assembly)
    {
        var rootCommand = root.Command;
        var blackboardSetter = typeof(BaseCommand)
            .GetProperty(nameof(BaseCommand.Blackboard))!
            .SetMethod!.CreateDelegate<Action<BaseCommand, Blackboard>>();
        foreach (var type in assembly.GetExportedTypes())
        {
            if (type.IsValueType || type.IsAbstract || type.IsInterface || !type.IsAssignableTo(typeof(BaseCommand)))
                continue;
            var instance = (BaseCommand?)Activator.CreateInstance(type);
            if (instance == null)
            {
                BaseCommand.Output($"Failed to instantiate command #{type}.", BaseCommand.Level.Warning);
                continue;
            }
            blackboardSetter(instance, root.Blackboard);
            rootCommand.AddCommand(instance.Command);
        }
    }

    public static bool Terminating { get; set; }= false;
    
    public static async Task<int> Main(string[] arguments)
    {
        var blackboard = new Blackboard();
        var commands = new ConnectCommand()
        {
            Blackboard = blackboard
        };
        
        Colorful.Console.WriteLine("Spot Console", Color.DarkOrange);
        
        InstallCommandsFromAssembly(commands, Assembly.GetExecutingAssembly());
        
        await commands.Command.InvokeAsync(arguments);
        
        while (!Terminating)
        {
            var text = BaseCommand.Input();
            await commands.Command.InvokeAsync(text);
        }
        
        return await commands.Command.InvokeAsync(arguments);
    }
}
