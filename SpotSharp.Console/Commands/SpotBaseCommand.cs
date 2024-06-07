namespace SpotSharp.Console.Commands;

public abstract class SpotBaseCommand(string name, string description) : BaseCommand(name, description)
{
    private SpotRobot? _robot = null;

    protected SpotRobot Robot
    {
        get
        {
            if (_robot != null)
                return _robot;
            _robot = Blackboard.Require<SpotRobot>();
            return _robot;
        }
    }
}