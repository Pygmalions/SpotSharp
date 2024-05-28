using Bosdyn.Api;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Type = System.Type;

namespace SpotSharp;

public partial class Robot : IDisposable
{
    public readonly string ClientName;

    public readonly Uri Address;

    private readonly Authenticator _authenticator;

    private readonly ILoggerFactory? _loggers;

    private Robot(Uri address, string client, Authenticator authenticator, ILoggerFactory? loggers)
    {
        Address = address;
        ClientName = client;
        _authenticator = authenticator;
        _loggers = loggers;
    }

    public void Dispose()
    {
        _services.Clear();
        foreach (var (_, channel) in _channels)
        {
            channel.Dispose();
        }
        _channels.Clear();
    }

    private readonly Dictionary<string, GrpcChannel> _channels = new();
    
    /// <summary>
    /// Get an authenticated channel according to the specific authority.
    /// </summary>
    /// <param name="authority">Authority, which is the address of the service.</param>
    /// <returns>Channel for the specific authority.</returns>
    public GrpcChannel GetChannel(string authority)
    {
        if (_channels.TryGetValue(authority, out var channel))
            return channel;
        channel = CreateChannel(Address, authority, _authenticator, _loggers);
        _channels[authority] = channel;
        return channel;
    }

    public async Task<GetAuthTokenResponse.Types.Status> UpdateToken()
        => await _authenticator.Update();

    /// <summary>
    /// Create a new request header according to the client name and current time.
    /// </summary>
    public RequestHeader Header => new RequestHeader()
    {
        ClientName = ClientName,
        RequestTimestamp = DateTime.UtcNow.ToTimestamp()
    };

    private readonly Dictionary<Type, ClientBase> _services = new();

    public TService GetService<TService>(string authority = "api.spot.robot") where TService : ClientBase
    {
        var type = typeof(TService);
        if (!_services.TryGetValue(type, out var service))
        {
            service = (TService?)Activator.CreateInstance(typeof(TService), [GetChannel(authority)])
                      ?? throw new ApplicationException(
                          $"Can not instantiate service client #{typeof(TService)}.");
            _services[type] = service;
        }

        return (TService)service;
    }
}