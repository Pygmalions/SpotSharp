using System.CommandLine;
using SpotSharp.Modules;

namespace SpotSharp.Console.Commands;

public class TakeLeaseCommand() : SpotBaseCommand("lease", "Require a lease.")
{
    protected override void SetupCommand(Command command)
    {
        var acquireCommand = new Command("acquire", "Acquire a lease.");
        var leasePart = new Argument<LeasePart>("part");
        acquireCommand.AddArgument(leasePart);
        acquireCommand.SetHandler(async part =>
        {
            await Robot.GetModule<LeaseManager>().Acquire(part);
            Output($"Lease for part '{part}' has been acquired.");
        }, leasePart);
        command.AddCommand(acquireCommand);
        
        var takeCommand = new Command("take", "Acquire a lease by force.");
        takeCommand.AddArgument(leasePart);
        takeCommand.SetHandler(async part =>
        {
            await Robot.GetModule<LeaseManager>().AcquireByForce(part);
            Output($"Lease for part '{part}' has been acquired.");
        }, leasePart);
        command.AddCommand(takeCommand);
        
        var statusCommand = new Command("status", "Show the status of the lease.");
        statusCommand.SetHandler(() =>
        {
            Output(Robot.GetModule<LeaseManager>().Lease.ToString());
        });
        command.AddCommand(statusCommand);
    }
}