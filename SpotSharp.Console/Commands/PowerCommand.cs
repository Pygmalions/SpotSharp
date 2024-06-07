using System.CommandLine;
using SpotSharp.Modules;

namespace SpotSharp.Console.Commands;

public class PowerCommand() : SpotBaseCommand("power", "Control the power of Spot")
{
    protected override void SetupCommand(Command command)
    {
        var onCommand = new Command("on", "Switch on the power of motors");
        onCommand.SetHandler(async () =>
        {
            await Robot.GetModule<PowerController>().SetMotorPower(true);
        });
        command.AddCommand(onCommand);
        
        var offCommand = new Command("off", "Switch off the power of motors");
        offCommand.SetHandler(async () =>
        {
            await Robot.GetModule<PowerController>().SetMotorPower(true);
        });
        command.AddCommand(offCommand);
    }
}