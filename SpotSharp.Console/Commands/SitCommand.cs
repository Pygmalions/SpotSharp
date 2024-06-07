using System.CommandLine;
using SpotSharp.Modules;

namespace SpotSharp.Console.Commands;

public class SitCommand() : SpotBaseCommand("sit", "Ask Spot to sit.")
{
    protected override void SetupCommand(Command command)
    {
        command.SetHandler(async () =>
        {
            await Robot.GetModule<Locomotion>().Sit();
        });
    }
}