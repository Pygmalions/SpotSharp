using Bosdyn.Api.Docking;
using Google.Protobuf.WellKnownTypes;

namespace SpotSharp.Modules;

public class DockManager : SpotModule
{
    private DockingService.DockingServiceClient _service = null!;
    
    protected internal override void Initialize()
    {
        _service = Robot.GetService<DockingService.DockingServiceClient>();
    }

    public async Task Dock(uint dockStationId)
    {
        var response = await _service.DockingCommandAsync(new DockingCommandRequest()
        {
            Header = Robot.Header,
            ClockIdentifier = Robot.GetModule<TimeSynchronizer>().ClockIdentifier,
            DockingStationId = dockStationId,
            EndTime = Robot.GetModule<TimeSynchronizer>().ToRobotTime(DateTime.UtcNow + TimeSpan.FromSeconds(20))
                .ToTimestamp(),
            Lease = Robot.GetModule<LeaseManager>().Lease,
        });
        if (response.Status != DockingCommandResponse.Types.Status.Ok)
            throw new RpcException($"Failed to dock.", response);
        
    }

    public async Task Undock()
    {
        var response = await _service.DockingCommandAsync(new DockingCommandRequest()
        {
            Header = Robot.Header,
            ClockIdentifier = Robot.GetModule<TimeSynchronizer>().ClockIdentifier,
            PrepPoseBehavior = PrepPoseBehavior.PrepPoseUndock,
            EndTime = Robot.GetModule<TimeSynchronizer>().ToRobotTime(DateTime.UtcNow + TimeSpan.FromSeconds(20))
                .ToTimestamp(),
            Lease = Robot.GetModule<LeaseManager>().Lease,
        });
        if (response.Status != DockingCommandResponse.Types.Status.Ok)
            throw new RpcException("Failed to undock.", response);
    }

    public async Task<uint> GetDockId()
    {
        var response = await _service.GetDockingStateAsync(new GetDockingStateRequest()
        {
            Header = Robot.Header
        });
        return response.DockState.DockId;
    }
}