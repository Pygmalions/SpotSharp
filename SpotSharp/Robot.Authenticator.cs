using Bosdyn.Api;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Metadata = Grpc.Core.Metadata;

namespace SpotSharp;

public partial class Robot
{
    private class Authenticator
    {
        public string ClientName;

        private readonly AuthService.AuthServiceClient _service;
        private string _token = string.Empty;
        private string _authorization = string.Empty;

        public DateTime Timestamp { get; private set; }
        
        private void BindToken(string token)
        {
            _token = token;
            _authorization = $"Bearer {token}";
        }
        
        private Authenticator(
            AuthService.AuthServiceClient service, string client, string token,
            DateTime timestamp)
        {
            ClientName = client;
            _service = service;
            BindToken(token);
            Timestamp = timestamp;
        }

        public async Task<GetAuthTokenResponse.Types.Status> Update()
        {
            var response = await _service.GetAuthTokenAsync(
                new GetAuthTokenRequest()
                {
                    Header = new RequestHeader()
                    {
                        ClientName = ClientName,
                        RequestTimestamp = DateTime.UtcNow.ToTimestamp()
                    },
                    Token = _token
                });
            if (response.Status == GetAuthTokenResponse.Types.Status.Ok)
                BindToken(response.Token);
            return response.Status;
        }

        public static async Task<Authenticator> Create(
            GrpcChannel channel, string client, string username, string password)
        {
            var service = new AuthService.AuthServiceClient(channel);
            var response = await service.GetAuthTokenAsync(new GetAuthTokenRequest
            {
                Header = new RequestHeader()
                {
                    ClientName = client,
                    RequestTimestamp = DateTime.UtcNow.ToTimestamp()
                },
                Username = username,
                Password = password
            });

            if (response.Status != GetAuthTokenResponse.Types.Status.Ok)
                throw new ApplicationException($"Failed to authenticate: {response.Status}.");

            return new Authenticator(service, client, response.Token, 
                response.Header.ResponseTimestamp.ToDateTime());
        }

        private Task Intercept(AuthInterceptorContext context, Metadata metadata)
        {
            metadata.Add("authorization", _authorization);
            return Task.CompletedTask;
        }

        public CallCredentials Credential => CallCredentials.FromInterceptor(Intercept);
    }
}