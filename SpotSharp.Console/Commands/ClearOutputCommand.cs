using System.CommandLine;

namespace SpotSharp.Console.Commands;

public class ClearOutputCommand() : BaseCommand("clear", "Clear Console Output")
{
    protected override void SetupCommand(Command command)
    {
        command.SetHandler(System.Console.Clear);
    }
}