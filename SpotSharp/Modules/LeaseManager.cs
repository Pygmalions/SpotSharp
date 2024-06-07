using Bosdyn.Api;

namespace SpotSharp.Modules;

public class LeaseManager : SpotModule
{
    private LeaseService.LeaseServiceClient _service = null!;

    protected internal override void Initialize()
    {
        _service = Robot.GetService<LeaseService.LeaseServiceClient>();
    }

    private Lease? _lease;

    public Lease Lease => _lease ?? throw new ApplicationException("Lease has not been acquired yet.");

    public bool HasLease => _lease != null;
    
    public static string LeasePartToResourceName(LeasePart part)
    {
        return part switch
        {
            LeasePart.Body => "body",
            LeasePart.FullArm => "full-arm",
            LeasePart.Mobility => "mobility",
            LeasePart.Gripper => "gripper",
            LeasePart.Arm => "arm",
            _ => throw new ArgumentOutOfRangeException(nameof(part), part, null)
        };
    }

    public static LeasePart LeasePartFromResourceName(string resourceName)
    {
        return resourceName switch
        {
            "body" => LeasePart.Body,
            "full-arm" => LeasePart.FullArm,
            "mobility" => LeasePart.Mobility,
            "gripper" => LeasePart.Gripper,
            "arm" => LeasePart.Arm,
            _ => throw new ArgumentOutOfRangeException(resourceName, resourceName, null)
        };
    }

    public async Task Acquire(LeasePart part)
    {
        var response = await _service.AcquireLeaseAsync(new()
        {
            Header = Robot.Header,
            Resource = LeasePartToResourceName(part)
        });

        if (response.Status != AcquireLeaseResponse.Types.Status.Ok)
            throw new RpcException("Failed to acquire lease.", response);

        _lease = response.Lease;
    }

    public async Task AcquireByForce(LeasePart part)
    {
        var response = await _service.TakeLeaseAsync(new TakeLeaseRequest()
        {
            Header = Robot.Header,
            Resource = LeasePartToResourceName(part)
        });

        if (response.Status != TakeLeaseResponse.Types.Status.Ok)
            throw new ApplicationException($"Failed to acquire lease: {response.Status}.");

        _lease = response.Lease;
    }

    public override void Dispose()
    {
        if (_lease != null)
            _service.ReturnLease(new ReturnLeaseRequest()
            {
                Header = Robot.Header,
                Lease = _lease
            });
    }
}