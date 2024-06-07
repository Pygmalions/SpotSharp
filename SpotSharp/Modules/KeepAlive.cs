using Bosdyn.Api.Keepalive;

namespace SpotSharp.Modules;

public class KeepAlive : SpotModule
{
    private KeepaliveService.KeepaliveServiceClient _service = null!;

    protected internal override void Initialize()
    {
        _service = Robot.GetService<KeepaliveService.KeepaliveServiceClient>();
    }

    public async Task CheckIn()
    {
        var status = await _service.GetStatusAsync(new GetStatusRequest()
        {
            Header = Robot.Header
        });
        await Task.WhenAll(status.Status.Select(async (policy) =>
            await _service.CheckInAsync(new CheckInRequest()
            {
                Header = Robot.Header,
                PolicyId = policy.PolicyId
            })));
    }
}