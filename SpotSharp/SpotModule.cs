namespace SpotSharp;

public abstract class SpotModule : IDisposable
{
    protected internal SpotModule() {}
    
    public required SpotRobot Robot { get; init; }

    protected internal virtual void Initialize()
    {}

    public virtual void Dispose()
    {}
}