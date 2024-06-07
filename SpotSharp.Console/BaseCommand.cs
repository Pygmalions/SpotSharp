using System.CommandLine;
using System.Drawing;

namespace SpotSharp.Console;

public abstract class BaseCommand
{
    public readonly string Name ;
    
    public readonly string? Description;

    public readonly bool IsRootCommand;

    public BaseCommand(string name, string? description = null, bool isRoot = false)
    {
        Name = name;
        Description = description;
        IsRootCommand = isRoot;
    }
    
    public required Blackboard Blackboard { init; get; }
    
    public enum Level
    {
        Debug,
        Information,
        Warning,
        Error
    }

    public static string Input()
    {
        Colorful.Console.Write("User > ", Color.CornflowerBlue);
        return Colorful.Console.ReadLine();
    }
    
    public static void Output(string text, Level level = Level.Information, string head = "Spot > ")
    {
        Colorful.Console.Write(head, Color.DarkOrange);
        if (level == Level.Information)
        {
            System.Console.WriteLine(text);
            return;
        }
        var color = level switch
        {
            Level.Debug => Color.Green,
            Level.Warning => Color.Gold,
            Level.Error => Color.DarkRed,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
        Colorful.Console.WriteLine(text, color);
    }

    protected abstract void SetupCommand(Command command);

    private Command? _command = null;

    public Command Command
    {
        get
        {
            if (_command != null)
                return _command;
            var command = IsRootCommand ? new RootCommand(Description ?? "") : new Command(Name, Description);
            SetupCommand(command);
            _command = command;
            return command;
        }
    }

    public static implicit operator Command(BaseCommand baseCommand) => baseCommand.Command;
}