using Bosdyn.Api;

namespace SpotSharp.Modules;

public class PowerController : SpotModule
{
    private PowerService.PowerServiceClient _service = null!;

    private LeaseManager _leaseManager = null!;
    
    protected internal override void Initialize()
    {
        _service = Robot.GetService<PowerService.PowerServiceClient>();
        _leaseManager = Robot.GetModule<LeaseManager>();
    }

    private uint _commandId = 0;

    public async Task SetMotorPower(bool powered)
    {
        var response = await _service.PowerCommandAsync(new PowerCommandRequest()
        {
            Header = Robot.Header,
            Lease = _leaseManager.Lease,
            Request = powered ? 
                PowerCommandRequest.Types.Request.OnMotors : 
                PowerCommandRequest.Types.Request.OffMotors
        });
        _commandId = response.PowerCommandId;
        if (response.Status != PowerCommandStatus.StatusSuccess &&
            response.Status != PowerCommandStatus.StatusInProgress)
            throw new RpcException("Failed to enable power of motors.", response);
    }

    public async Task<PowerCommandStatus> GetMotorPower()
    {
        var response = await _service.PowerCommandFeedbackAsync(new PowerCommandFeedbackRequest()
        {
            Header = Robot.Header,
            PowerCommandId = _commandId,
        });
        return response.Status;
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}