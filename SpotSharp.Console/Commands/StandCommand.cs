using System.CommandLine;
using SpotSharp.Modules;

namespace SpotSharp.Console.Commands;

public class StandCommand() : SpotBaseCommand("stand", "Ask Spot to stand.")
{
    protected override void SetupCommand(Command command)
    {
        command.SetHandler(async () =>
        {
            await Robot.GetModule<Locomotion>().Stand();
        });
    }
}