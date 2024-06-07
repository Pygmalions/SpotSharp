using System.CommandLine;
using SpotSharp.Modules;

namespace SpotSharp.Console.Commands;

public class SynchronizeTimeCommand() : SpotBaseCommand("sync-time", "Synchronize the time between client and Spot.")
{
    protected override void SetupCommand(Command command)
    {
        command.SetHandler(async () =>
        {
            var module = Robot.GetModule<TimeSynchronizer>();
            await module.Synchronize();
            Output("Time synchronization information:");
            Output($"Clock Identifier: {module.ClockIdentifier}", head: " > ");
            Output($"Best Estimation: {module.BestEstimation}", head: " > ");
            Output($"Best Estimation Timestamp: {module.BestEstimationTimestamp}", head: " > ");
        });
    }
}