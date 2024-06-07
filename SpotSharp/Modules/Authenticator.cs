using Bosdyn.Api;
using Grpc.Core;

namespace SpotSharp.Modules;

public class Authenticator : SpotModule
{
    private readonly AuthService.AuthServiceClient _service;
    
    private string _token = string.Empty;
    private string _authorization = string.Empty;
    
    private Authenticator(AuthService.AuthServiceClient service, string token)
    {
        _service = service;
        BindToken(token);
    }

    internal static async Task<Authenticator> Create(SpotRobot robot, string username, string password)
    {
        var service = new AuthService.AuthServiceClient(robot.GetChannel("auth.spot.robot"));
        
        var response = await service.GetAuthTokenAsync(new GetAuthTokenRequest
        {
            Header = robot.Header,
            Username = username,
            Password = password
        });

        if (response.Status != GetAuthTokenResponse.Types.Status.Ok)
            throw new RpcException($"Failed to authenticate.", response);
        var authenticator = new Authenticator(service, response.Token)
        {
            Robot = robot
        };
        return authenticator;
    }
    
    private void BindToken(string token)
    {
        _token = token;
        _authorization = $"Bearer {token}";
    }
    
    public async Task<GetAuthTokenResponse.Types.Status> Update()
    {
        var response = await _service.GetAuthTokenAsync(
            new GetAuthTokenRequest()
            {
                Header = Robot.Header,
                Token = _token
            });
        if (response.Status == GetAuthTokenResponse.Types.Status.Ok)
            BindToken(response.Token);
        return response.Status;
    }

    protected internal override void Initialize()
    {
        Robot.MessageTransmitting += Intercept;
    }

    public override void Dispose()
    {
        Robot.MessageTransmitting -= Intercept;
    }

    private Task Intercept(AuthInterceptorContext context, Grpc.Core.Metadata metadata)
    {
        metadata.Add("authorization", _authorization);
        return Task.CompletedTask;
    }
}

public static class AuthenticatorExtension
{
    public static async Task Authenticate(this SpotRobot robot, string username, string password)
    {
        var authenticator = await Authenticator.Create(robot, username, password);
        robot.InstallModule(typeof(Authenticator), authenticator);
    }
}