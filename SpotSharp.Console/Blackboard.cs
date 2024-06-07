namespace SpotSharp.Console;

public class Blackboard
{
    private readonly Dictionary<Type, object> _values = new();

    public TValue? Get<TValue>() => (TValue?)_values.GetValueOrDefault(typeof(TValue));

    public void Set<TValue>(TValue? value)
    {
        if (value != null)
            _values[typeof(TValue)] = value;
        else _values.Remove(typeof(TValue));
    }
    
    public object? this[Type type]
    {
        get => _values.GetValueOrDefault(type);
        set
        {
            if (value == null)
                _values.Remove(type);
            else _values[type] = value;
        }
    }
}

public static class BlackboardExtension
{
    public static TValue Require<TValue>(this Blackboard blackboard)
        => blackboard.Get<TValue>() ?? throw new ApplicationException(
            $"Can't find required value #{typeof(TValue)} in blackboard. " +
            $"You may need to enter some pre-commands before this command.");
}