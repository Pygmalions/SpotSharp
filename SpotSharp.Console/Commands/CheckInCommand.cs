using System.CommandLine;
using SpotSharp.Modules;

namespace SpotSharp.Console.Commands;

public class CheckInCommand() : SpotBaseCommand("check-in", "Check in for keep alive system")
{
    protected override void SetupCommand(Command command)
    {
        command.SetHandler(async () =>
        {
            await Robot.GetModule<KeepAlive>().CheckIn();
        });
    }
}