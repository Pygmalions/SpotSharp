using Bosdyn.Api;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SpotSharp.Modules;
using Type = System.Type;

namespace SpotSharp;

public class SpotRobot : IDisposable
{
    public readonly string ClientName;

    public readonly Uri Address;

    public ILoggerFactory? LoggerFactory { get; set; }
    
    private SpotRobot(Uri address, string clientName)
    {
        Address = address;
        ClientName = clientName;
    }

    public event AsyncAuthInterceptor? MessageTransmitting;
    
    private readonly Dictionary<string, GrpcChannel> _channels = new();
    
    public RequestHeader Header => new RequestHeader()
    {
        ClientName = ClientName,
        RequestTimestamp = DateTime.UtcNow.ToTimestamp()
    };
    
    /// <summary>
    /// Get an authenticated channel according to the specific authority.
    /// </summary>
    /// <param name="authority">Authority, which is the address of the service.</param>
    /// <returns>Channel for the specific authority.</returns>
    public GrpcChannel GetChannel(string authority)
    {
        if (_channels.TryGetValue(authority, out var channel))
            return channel;
        channel = SpotChannel.Create(Address, authority, 
            CallCredentials.FromInterceptor(Intercept), LoggerFactory);
        _channels[authority] = channel;
        return channel;
    }
    
    private readonly Dictionary<Type, SpotModule> _modules = new();

    internal void InstallModule(Type category, SpotModule module)
    {
        if (_modules.Remove(category, out var previous))
            previous.Dispose();
        _modules[category] = module;
        module.Initialize();
    }
    
    public TModule GetModule<TModule>() where TModule : SpotModule
    {
        if (_modules.TryGetValue(typeof(TModule), out var module))
            return (TModule)module;
        module = Activator.CreateInstance<TModule>();
        // Inject this robot into the robot field of the module.
        typeof(TModule).GetProperty("Robot")!.SetValue(module, this);
        
        InstallModule(typeof(TModule), module);
        
        return (TModule)module;
    }

    public void Dispose()
    {
        foreach (var (_, module) in _modules)
            module.Dispose();
        _modules.Clear();
    }
    
    private Task Intercept(AuthInterceptorContext context, Grpc.Core.Metadata metadata)
    {
        return MessageTransmitting?.Invoke(context, metadata) ?? Task.CompletedTask;
    }

    public static SpotRobot Connect(Uri address, string clientName, ILoggerFactory? loggerFactory = null)
    {
        var robot = new SpotRobot(address, clientName)
        {
            LoggerFactory = loggerFactory
        };
        return robot;
    }
}