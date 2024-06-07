using System.Numerics;
using Bosdyn.Api;

namespace SpotSharp.Modules;

public class Locomotion : SpotModule
{
    private RobotCommandService.RobotCommandServiceClient _service = null!;

    private TimeSynchronizer _timeSynchronizer = null!;

    private LeaseManager _leaseManager = null!;

    protected internal override void Initialize()
    {
        _service = Robot.GetService<RobotCommandService.RobotCommandServiceClient>();
        _timeSynchronizer = Robot.GetModule<TimeSynchronizer>();
        _leaseManager = Robot.GetModule<LeaseManager>();
    }

    public async Task<RobotCommandResponse> SendCommand(RobotCommand command)
    {
        return await _service.RobotCommandAsync(new RobotCommandRequest
        {
            Header = Robot.Header,
            ClockIdentifier = _timeSynchronizer.ClockIdentifier,
            Lease = _leaseManager.Lease,
            Command = command
        });
    }

    public readonly SE2Velocity SlewRateLimitation = new()
    {
        Linear = new Vec2() { X = 4, Y = 4 },
        Angular = 2.0
    };

    public async Task Move(Vector2 linearVelocity, double angularVelocity, TimeSpan duration)
    {
        await SendCommand(new RobotCommand
        {
            SynchronizedCommand = new SynchronizedCommand.Types.Request
            {
                MobilityCommand = new MobilityCommand.Types.Request
                {
                    Se2VelocityRequest = new SE2VelocityCommand.Types.Request
                    {
                        EndTime = _timeSynchronizer.ToRobotTimestamp(DateTime.UtcNow + duration),
                        Se2FrameName = "body",
                        SlewRateLimit = SlewRateLimitation,
                        Velocity = new SE2Velocity
                        {
                            Linear = new Vec2 { X = linearVelocity.X, Y = linearVelocity.Y },
                            Angular = angularVelocity
                        }
                    }
                }
            }
        });
    }

    public async Task Sit()
    {
        var response = await SendCommand(new RobotCommand
        {
            SynchronizedCommand = new SynchronizedCommand.Types.Request
            {
                MobilityCommand = new MobilityCommand.Types.Request
                {
                    SitRequest = new()
                }
            }
        });
        if (response.Status != RobotCommandResponse.Types.Status.Ok)
            throw new RpcException("Failed to sit.", response);
        
    }

    public async Task Stand()
    {
        var response = await SendCommand(new RobotCommand
        {
            SynchronizedCommand = new SynchronizedCommand.Types.Request
            {
                MobilityCommand = new MobilityCommand.Types.Request
                {
                    StandRequest = new()
                }
            }
        });
        if (response.Status != RobotCommandResponse.Types.Status.Ok)
            throw new RpcException("Failed to stand.", response);
    }
}