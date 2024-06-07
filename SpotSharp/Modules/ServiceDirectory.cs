using System.Text;
using Bosdyn.Api;
using Grpc.Core;

namespace SpotSharp.Modules;

public class ServiceDirectory : SpotModule
{
    private DirectoryService.DirectoryServiceClient _service = null!;

    protected internal override void Initialize()
    {
        _service = new DirectoryService.DirectoryServiceClient(Robot.GetChannel("api.spot.robot"));
        Refresh().Wait();
    }

    private readonly Dictionary<Type, ClientBase> _clients = new();

    private readonly Dictionary<Type, ServiceEntry> _entries = new();

    public IReadOnlyDictionary<Type, ServiceEntry> Entries => _entries;

    public TService GetService<TService>() where TService : ClientBase
    {
        if (_clients.TryGetValue(typeof(TService), out var client))
            return (TService)client;
        var entryType = typeof(TService).DeclaringType;
        if (entryType == null)
            throw new ApplicationException(
                $"Client type #{typeof(TService)} is not a proper gRPC client type.");
        if (!_entries.TryGetValue(entryType, out var entry))
            Refresh().Wait();
        if (!_entries.TryGetValue(entryType, out entry))
            throw new ApplicationException($"Can not find service entry for '{typeof(TService)}'.");
        client = (TService?)Activator.CreateInstance(typeof(TService), Robot.GetChannel(entry.Authority));
        _clients[typeof(TService)] =
            client ?? throw new ApplicationException($"Failed to instantiate service '{typeof(TService)}'");
        return (TService)client;
    }

    private static string UnderlineCaseToCamelCase(StringBuilder builder, string underlineName)
    {
        var needCapitalized = true;
        foreach (var character in underlineName)
        {
            switch (character)
            {
                case '_':
                    needCapitalized = true;
                    continue;
                case '.':
                    needCapitalized = true;
                    builder.Append(character);
                    continue;
                default:
                    if (!needCapitalized)
                        builder.Append(character);
                    else
                    {
                        builder.Append(char.ToUpperInvariant(character));
                        needCapitalized = false;
                    }
                    break;
            }
        }
        var camelName = builder.ToString();
        builder.Clear();
        return camelName;
    }

    public async Task Refresh()
    {
        var response = await _service.ListServiceEntriesAsync(new ListServiceEntriesRequest()
        {
            Header = Robot.Header
        });

        var typeNameBuilder = new StringBuilder();
        foreach (var entry in response.ServiceEntries)
        {
            entry.Type = UnderlineCaseToCamelCase(typeNameBuilder, entry.Type);
            var type = Type.GetType(entry.Type);
            if (type == null)
                // Todo: add to logs.
                continue;
            _entries[type] = entry;
        }
    }
}

public static class ServiceDirectoryExtension
{
    public static TService GetService<TService>(this SpotRobot robot) where TService : ClientBase
        => robot.GetModule<ServiceDirectory>().GetService<TService>();
}