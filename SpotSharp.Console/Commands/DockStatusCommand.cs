using System.CommandLine;
using SpotSharp.Modules;

namespace SpotSharp.Console.Commands;

public class DockStatusCommand() : SpotBaseCommand("dock-status", "Show the configurations of dock.")
{
    protected override void SetupCommand(Command command)
    {
        command.SetHandler(async () =>
        {
            var id = await Robot.GetModule<DockManager>().GetDockId();
            Output($"Dock ID: {id}");
        });
    }
}