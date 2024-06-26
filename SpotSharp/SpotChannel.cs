﻿using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace SpotSharp;

public static class SpotChannel
{
    private static readonly Lazy<X509Certificate2> Certificate = new(() =>
        X509Certificate2.CreateFromPem(
            """
            -----BEGIN CERTIFICATE-----
            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
            -----END CERTIFICATE-----
            """));

    public static GrpcChannel Create(
        Uri address, string authority,
        CallCredentials? authenticator = null, ILoggerFactory? logger = null)
    {
        var handler = new SocketsHttpHandler
        {
            ConnectCallback = async (context, cancellationToken) =>
            {
                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
                try
                {
                    await socket.ConnectAsync(context.DnsEndPoint, cancellationToken);

                    var stream = new SslStream(new NetworkStream(socket, ownsSocket: true));

                    await stream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
                    {
                        ApplicationProtocols = new List<SslApplicationProtocol>
                        {
                            SslApplicationProtocol.Http2
                        },
                        TargetHost = authority,
                        ClientCertificates = new X509Certificate2Collection(Certificate.Value),
                        CertificateChainPolicy = new X509ChainPolicy()
                        {
                            TrustMode = X509ChainTrustMode.CustomRootTrust,
                            CustomTrustStore = { Certificate.Value },
                            RevocationMode = X509RevocationMode.NoCheck
                        },
                    }, cancellationToken);
                    return stream;
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            },
        };
        var client = new HttpClient(handler, true);
        client.DefaultRequestHeaders.Host = authority;

        var credentials = authenticator == null
            ? ChannelCredentials.SecureSsl
            : ChannelCredentials.Create(ChannelCredentials.SecureSsl, authenticator);

        return GrpcChannel.ForAddress(address, new GrpcChannelOptions()
        {
            Credentials = credentials,
            HttpClient = client,
            LoggerFactory = logger,
            DisposeHttpClient = true
        });
    }
}