using System.CommandLine;

namespace SpotSharp.Console.Commands;

public class ExitCommand() : BaseCommand("exit", "Exit the console.")
{
    protected override void SetupCommand(Command command)
    {
        command.SetHandler(() =>
        {
            Launcher.Terminating = true;
        });
    }
}