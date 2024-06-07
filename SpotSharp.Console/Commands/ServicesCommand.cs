using System.CommandLine;
using SpotSharp.Modules;

namespace SpotSharp.Console.Commands;

public class ServicesCommand() : SpotBaseCommand("services", "List all available services.")
{
    protected override void SetupCommand(Command command)
    {
        command.SetHandler(async () =>
        {
            var directory = Robot.GetModule<ServiceDirectory>();
            await directory.Refresh();
            Output("Available services: ");
            foreach (var (_, entry) in directory.Entries)
            {
                Output($"{entry.Name} - #{entry.Type} @{entry.Authority} | {entry.PermissionRequired}", head: " > ");
            }
        }); 
    }
}