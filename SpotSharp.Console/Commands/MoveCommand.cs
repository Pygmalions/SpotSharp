using System.CommandLine;
using System.Numerics;
using SpotSharp.Modules;

namespace SpotSharp.Console.Commands;

public class MoveCommand() : SpotBaseCommand("move", "Ask Spot to move")
{
    protected override void SetupCommand(Command command)
    {
        var xVelocity = new Argument<float>("x_velocity");
        var yVelocity = new Argument<float>("y_velocity");
        var argumentMilliseconds = new Argument<double>("milliseconds");
        command.AddArgument(xVelocity);
        command.AddArgument(yVelocity);
        command.AddArgument(argumentMilliseconds);
        command.SetHandler(async (x, y, milliseconds) =>
        {
            await Robot.GetModule<Locomotion>()
                .Move(new Vector2(x, y), 0.0, 
                    TimeSpan.FromMilliseconds(milliseconds));
        }, xVelocity, yVelocity, argumentMilliseconds);
    }
}