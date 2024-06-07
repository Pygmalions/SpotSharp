using Bosdyn.Api;
using Google.Protobuf.WellKnownTypes;

namespace SpotSharp.Modules;

public class TimeSynchronizer : SpotModule
{
    private TimeSyncService.TimeSyncServiceClient _service = null!;

    protected internal override void Initialize()
    {
        _service = Robot.GetService<TimeSyncService.TimeSyncServiceClient>();
        Update().Wait();
    }

    public TimeSyncEstimate BestEstimation { get; private set; } = new();
    public Timestamp BestEstimationTimestamp { get; private set; } = new();
    
    public string? ClockIdentifier { get; private set; }
    
    private readonly TimeSyncRoundTrip _previousRoundTrip = new();
    
    public async Task<TimeSyncState> Update()
    {
        var request = new TimeSyncUpdateRequest()
        {
            Header = Robot.Header,
        };
        if (ClockIdentifier != null)
            request.ClockIdentifier = ClockIdentifier;
        if (_previousRoundTrip.ClientTx != null)
            request.PreviousRoundTrip = _previousRoundTrip;

        var response = await _service.TimeSyncUpdateAsync(request);

        _previousRoundTrip.ClientRx = DateTime.UtcNow.ToTimestamp();
        _previousRoundTrip.ClientTx = response.Header.RequestHeader.RequestTimestamp;
        _previousRoundTrip.ServerRx = response.Header.RequestReceivedTimestamp;
        _previousRoundTrip.ServerTx = response.Header.ResponseTimestamp;

        ClockIdentifier = response.ClockIdentifier;

        BestEstimation = response.State.BestEstimate;
        BestEstimationTimestamp = response.State.MeasurementTime;

        return response.State;
    }

    /// <summary>
    /// Invoke <see cref="Update"/> for multiples times.
    /// Set <see cref="MaxSampleCount"/> to control the max count of update attempts.
    /// </summary>
    public async Task Synchronize()
    {
        var currentSamples = 0;
        while (currentSamples < MaxSampleCount)
        {
            var state = await Update();
            if (state.Status == TimeSyncState.Types.Status.Ok)
                break;
            currentSamples++;
        }
    }
    
    private Timer? _backgroundUpdater;

    private TimeSpan? _updaterIntervalTime;
    
    /// <summary>
    /// Check if background synchronizer is currently running.
    /// </summary>
    public bool IsUpdateRunning => _backgroundUpdater != null;
    
    /// <summary>
    /// Set this property to a non-null value to enable or change the period of background synchronizer.
    /// Set it to null to disable the background synchronizer.
    /// </summary>
    public TimeSpan? UpdaterIntervalTime
    {
        get => _updaterIntervalTime;
        set
        {
            if (value == null)
            {
                // Dispose background updater.
                _backgroundUpdater?.Dispose();
                _backgroundUpdater = null;
                _updaterIntervalTime = value;
            }
            else
            {
                if (_backgroundUpdater == null)
                    // Instantiate background updater.
                    _backgroundUpdater = new Timer(async _ =>
                    {
                        await Synchronize();
                    }, null, TimeSpan.Zero, value.Value);
                else _backgroundUpdater.Change(TimeSpan.Zero, value.Value);
                _updaterIntervalTime = value;
            }
        }
    }

    public DateTime ToRobotTime(DateTime localTime)
    {
        return localTime + BestEstimation.ClockSkew.ToTimeSpan();
    }

    public Timestamp ToRobotTimestamp(DateTime localtime)
    {
        return ToRobotTime(localtime).ToTimestamp();
    }
    
    /// <summary>
    /// Max count of update attempts in a synchronization.
    /// </summary>
    public static int MaxSampleCount { get; set; } = 5;

    public override void Dispose()
    {
        UpdaterIntervalTime = null;
    }
}