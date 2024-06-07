using System.CommandLine;
using SpotSharp.Modules;

namespace SpotSharp.Console.Commands;

public class UndockCommand() : SpotBaseCommand("undock", "Undock Spot from dock station")
{
    protected override void SetupCommand(Command command)
    {
        command.SetHandler(async () =>
        {
            await Robot.GetModule<DockManager>().Undock();
        });
    }
}