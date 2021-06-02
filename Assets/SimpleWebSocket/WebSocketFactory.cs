using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using JamesFrowen.SimpleWeb;
using Mirage.SocketLayer;
using UnityEngine;
using EventType = JamesFrowen.SimpleWeb.EventType;

namespace JamesFrowen.Mirage.Sockets.SimpleWeb
{
    public sealed class WebSocketFactory : SocketFactory
    {
        public string address = "localhost";
        public int port = 7777;
        public TcpConfig tcpConfig;

        [Header("Ssl Settings")]
        [Tooltip("Sets connect scheme to wss. Useful when client needs to connect using wss when TLS is outside of transport, NOTE: if sslEnabled is true clientUseWss is also true")]
        public bool clientUseWss;

        public bool sslEnabled;
        [Tooltip("Path to json file that contains path to cert and its password\n\nUse Json file so that cert password is not included in client builds\n\nSee cert.example.Json")]
        public string sslCertJson = "./cert.json";
        public SslProtocols sslProtocols = SslProtocols.Tls12;

        public override ISocket CreateClientSocket()
        {
            // todo get max message size somewhere else?
            SslConfig sslConfig = SslConfigLoader.Load(sslEnabled || clientUseWss, sslCertJson, sslProtocols);
            return new ClientWebSocket(tcpConfig, new Config().BufferSize, sslConfig);
        }

        public override ISocket CreateServerSocket()
        {
            if (IsWebgl)
            {
                throw new NotSupportedException("Webgl can not be a server");
            }

            // todo get max message size somewhere else?
            SslConfig sslConfig = SslConfigLoader.Load(sslEnabled || clientUseWss, sslCertJson, sslProtocols);
            return new ServerWebSocket(tcpConfig, new Config().BufferSize, sslConfig);
        }

        public override EndPoint GetBindEndPoint()
        {
            return new IPEndPoint(IPAddress.IPv6Any, port);
        }

        public override EndPoint GetConnectEndPoint(string address = null, ushort? port = null)
        {
            string addressString = address ?? this.address;
            IPAddress ipAddress = getAddress(addressString);

            ushort portIn = port ?? (ushort)this.port;

            return new IPEndPoint(ipAddress, portIn);
        }

        private IPAddress getAddress(string addressString)
        {
            if (IPAddress.TryParse(addressString, out IPAddress address))
                return address;

            IPAddress[] results = Dns.GetHostAddresses(addressString);
            if (results.Length == 0)
            {
                throw new SocketException((int)SocketError.HostNotFound);
            }
            else
            {
                return results[0];
            }
        }

        private static bool IsWebgl => Application.platform == RuntimePlatform.WebGLPlayer;
    }
    public class SimpleWebEndPoint : EndPoint, IEquatable<SimpleWebEndPoint>
    {
        public string address;
        public int port;
        internal int ConnectionId;

        public bool Equals(SimpleWebEndPoint other)
        {
            throw new NotImplementedException();
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    public enum SimpleWebSocketDisconnectReasons
    {
        /// <summary>
        /// Socket was closed not by 
        /// </summary>
        SocketClosed = 14,

        /// <summary>
        /// Socket was closed not by 
        /// </summary>
        SocketError = 15,
    }
    public class ServerWebSocket : ISocket
    {
        private readonly TcpConfig tcpConfig;
        private readonly int maxMessageSize;
        private readonly SslConfig sslConfig;
        const int handshakeMaxSize = 3000;

        private JamesFrowen.SimpleWeb.BufferPool pool;
        private WebSocketServer server;

        Dictionary<int, SimpleWebEndPoint> endpoints = new Dictionary<int, SimpleWebEndPoint>();

        public ServerWebSocket(TcpConfig tcpConfig, int maxMessageSize, SslConfig sslConfig)
        {
            this.tcpConfig = tcpConfig;
            this.maxMessageSize = maxMessageSize;
            this.sslConfig = sslConfig;
        }

        public void Bind(EndPoint endPoint)
        {
            pool = new JamesFrowen.SimpleWeb.BufferPool(5, 20, maxMessageSize);
            server = new WebSocketServer(tcpConfig, maxMessageSize, handshakeMaxSize, sslConfig, pool);

            var swEndPoint = (SimpleWebEndPoint)endPoint;
            server.Listen(swEndPoint.port);
        }

        public void Close()
        {
            server?.Stop();
        }

        public void Connect(EndPoint endPoint)
        {
            throw new NotSupportedException();
        }

        public bool Poll()
        {
            return server.receiveQueue.Count > 0;
        }

        public int Receive(byte[] buffer, out EndPoint endPoint)
        {
            bool result = server.receiveQueue.TryDequeue(out Message next);
            if (!result)
            {
                throw new InvalidOperationException("No Packets in queue");
            }

            switch (next.type)
            {
                case EventType.Connected:
                    {

                        // do nothing with connected event?
                        // Peer will do its own handshake using data
                        var swEndPoint = new SimpleWebEndPoint() { ConnectionId = next.connId };
                        endPoint = swEndPoint;
                        endpoints.Add(next.connId, swEndPoint);

                        // use keep alive so this packet can just be thrown away
                        buffer[0] = (byte)PacketType.KeepAlive;
                        return 1;
                    }
                case EventType.Data:
                    {
                        SimpleWebEndPoint swEndPoint = endpoints[next.connId];
                        endPoint = swEndPoint;

                        next.data.CopyTo(buffer, 0);
                        int count = next.data.count;
                        next.data.Release();
                        return count;
                    }
                case EventType.Error:
                case EventType.Disconnected:
                    {
                        if (endpoints.TryGetValue(next.connId, out SimpleWebEndPoint swEndPoint))
                        {
                            endPoint = swEndPoint;
                            endpoints.Remove(next.connId);

                            buffer[0] = (byte)PacketType.Command;
                            buffer[1] = (byte)Commands.Disconnect;
                            buffer[2] = (byte)(next.type == EventType.Disconnected ? SimpleWebSocketDisconnectReasons.SocketClosed : SimpleWebSocketDisconnectReasons.SocketError);
                            return 3;
                        }
                        else
                        {
                            break;
                        }
                    }
            }

            // default, either endpoint not found or error.
            endPoint = null;
            return 0;
        }

        public void Send(EndPoint endPoint, byte[] packet, int length)
        {
            var swEndPoint = (SimpleWebEndPoint)endPoint;

            ArrayBuffer buffer = pool.Take(length);
            buffer.CopyFrom(packet, 0, length);

            server.Send(swEndPoint.ConnectionId, buffer);
        }
    }
    public class ClientWebSocket : ISocket
    {
        public const string NormalScheme = "ws";
        public const string SecureScheme = "wss";

        private readonly TcpConfig tcpConfig;
        private readonly int maxMessageSize;
        private readonly SslConfig sslConfig;

        private JamesFrowen.SimpleWeb.BufferPool pool;
        private SimpleWebClient client;

        SimpleWebEndPoint remoteEndpoint;

        public ClientWebSocket(TcpConfig tcpConfig, int maxMessageSize, SslConfig sslConfig)
        {
            this.tcpConfig = tcpConfig;
            this.maxMessageSize = maxMessageSize;
            this.sslConfig = sslConfig;
        }

        public void Bind(EndPoint endPoint)
        {
            throw new NotSupportedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Connect(EndPoint endPoint)
        {
            client = SimpleWebClient.Create(maxMessageSize, 10_000, tcpConfig);

            remoteEndpoint = (SimpleWebEndPoint)endPoint;
            var builder = new UriBuilder
            {
                Scheme = GetClientScheme(),
                Host = remoteEndpoint.address,
                Port = remoteEndpoint.port
            };

            client.Connect(builder.Uri);
        }
        string GetClientScheme() => (sslConfig.enabled) ? SecureScheme : NormalScheme;

        public bool Poll()
        {
            return client.receiveQueue.Count > 0;
        }

        public int Receive(byte[] buffer, out EndPoint endPoint)
        {
            endPoint = remoteEndpoint;

            bool result = client.receiveQueue.TryDequeue(out Message next);
            if (!result)
            {
                throw new InvalidOperationException("No Packets in queue");
            }

            switch (next.type)
            {
                case EventType.Connected:
                    {
                        // do nothing with connected event?
                        // Peer will do its own handshake using data
                        remoteEndpoint = new SimpleWebEndPoint();

                        // use keep alive so this packet can just be thrown away
                        buffer[0] = (byte)PacketType.KeepAlive;
                        return 1;
                    }
                case EventType.Data:
                    {
                        next.data.CopyTo(buffer, 0);
                        int count = next.data.count;
                        next.data.Release();

                        return count;
                    }
                case EventType.Error:
                case EventType.Disconnected:
                    {
                        buffer[0] = (byte)PacketType.Command;
                        buffer[1] = (byte)Commands.Disconnect;
                        buffer[2] = (byte)(next.type == EventType.Disconnected ? SimpleWebSocketDisconnectReasons.SocketClosed : SimpleWebSocketDisconnectReasons.SocketError);
                        return 3;
                    }
            }

            // default, either endpoint not found or error.
            endPoint = null;
            return 0;
        }

        public void Send(EndPoint endPoint, byte[] packet, int length)
        {
            Debug.Assert(endPoint == remoteEndpoint);
            client.Send(new ArraySegment<byte>(packet, 0, length));
        }
    }
}
