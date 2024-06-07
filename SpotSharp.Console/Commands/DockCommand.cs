using System.CommandLine;
using SpotSharp.Modules;

namespace SpotSharp.Console.Commands;

public class DockCommand() : SpotBaseCommand("dock", "Dock Spot to a dock station.")
{
    protected override void SetupCommand(Command command)
    {
        var stationId = new Argument<uint>("station", "Station ID to dock to.");
        command.AddArgument(stationId);
        command.SetHandler(async (id) =>
        {
            await Robot.GetModule<DockManager>().Dock(id);
        }, stationId);
    }
}