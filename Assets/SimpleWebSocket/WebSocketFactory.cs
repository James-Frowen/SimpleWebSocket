using System;
using System.Security.Authentication;
using JamesFrowen.SimpleWeb;
using Mirage.SocketLayer;
using UnityEngine;
using EventType = JamesFrowen.SimpleWeb.EventType;

namespace JamesFrowen.Mirage.Sockets.SimpleWeb
{
    public sealed class WebSocketFactory : SocketFactory, IHasAddress, IHasPort
    {
        string IHasAddress.Address { get => address; set => address = value; }
        int IHasPort.Port { get => port; set => port = value; }

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

        [Header("Debug")]
        [Tooltip("Log functions uses ConditionalAttribute which will effect which log methods are allowed. DEBUG allows warn/error, SIMPLEWEB_LOG_ENABLED allows all")]
        [SerializeField] Log.Levels _logLevels = Log.Levels.none;

        /// <summary>
        /// <para>Gets _logLevels field</para>
        /// <para>Sets _logLevels and Log.level fields</para>
        /// </summary>
        public Log.Levels LogLevels
        {
            get => _logLevels;
            set
            {
                _logLevels = value;
                Log.level = _logLevels;
            }
        }

        void OnValidate()
        {
            Log.level = _logLevels;
        }
        void Awake()
        {
            Log.level = _logLevels;
        }

        public override ISocket CreateClientSocket()
        {
            // todo get max message size somewhere else?
            SslConfig sslConfig = SslConfigLoader.Load(sslEnabled || clientUseWss, sslCertJson, sslProtocols);
            return new ClientWebSocket(tcpConfig, new Config().MaxPacketSize, sslConfig);
        }

        public override ISocket CreateServerSocket()
        {
            if (IsWebgl)
            {
                throw new NotSupportedException("Webgl can not be a server");
            }

            // todo get max message size somewhere else?
            SslConfig sslConfig = SslConfigLoader.Load(sslEnabled || clientUseWss, sslCertJson, sslProtocols);
            return new ServerWebSocket(tcpConfig, new Config().MaxPacketSize, sslConfig);
        }

        public override IEndPoint GetBindEndPoint()
        {
            return new SimpleWebEndPoint(default, port);
        }

        public override IEndPoint GetConnectEndPoint(string address = null, ushort? port = null)
        {
            string addressString = address ?? this.address;
            ushort portIn = port ?? (ushort)this.port;

            return new SimpleWebEndPoint(addressString, portIn);
        }

        private static bool IsWebgl => Application.platform == RuntimePlatform.WebGLPlayer;
    }

    public class SimpleWebEndPoint : IEndPoint
    {
        public string HostName;
        public ushort Port;
        internal int ConnectionId;

        public SimpleWebEndPoint(string hostName, int port) : this(hostName, checked((ushort)port)) { }
        public SimpleWebEndPoint(string hostName, ushort port)
        {
            HostName = hostName;
            Port = port;
        }
        internal SimpleWebEndPoint() { }
        private SimpleWebEndPoint(SimpleWebEndPoint other)
        {
            HostName = other.HostName;
            Port = other.Port;
            ConnectionId = other.ConnectionId;
        }

        public override bool Equals(object obj)
        {
            if (obj is SimpleWebEndPoint other)
            {
                if (ConnectionId == other.ConnectionId)
                {
                    // if it has id, then they are the same
                    if (ConnectionId != 0)
                        return true;

                    // if no idea, check if other props are equal
                    return HostName.Equals(other.HostName) && Port.Equals(other.Port);
                }


                return false;
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (ConnectionId != 0)
                return ConnectionId;

            return HostName.GetHashCode() ^ Port.GetHashCode();
        }

        public override string ToString()
        {
            if (ConnectionId != 0)
                return $"Active connection:{ConnectionId}";
            else
                return $"{HostName}:{Port}";
        }

        IEndPoint IEndPoint.CreateCopy()
        {
            return new SimpleWebEndPoint(this);
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

        SimpleWebEndPoint ReceiveEndpoint = new SimpleWebEndPoint();

        public ServerWebSocket(TcpConfig tcpConfig, int maxMessageSize, SslConfig sslConfig)
        {
            this.tcpConfig = tcpConfig;
            this.maxMessageSize = maxMessageSize;
            this.sslConfig = sslConfig;
        }

        public void Bind(IEndPoint endPoint)
        {
            pool = new JamesFrowen.SimpleWeb.BufferPool(5, 20, maxMessageSize);
            server = new WebSocketServer(tcpConfig, maxMessageSize, Math.Min(maxMessageSize, handshakeMaxSize), sslConfig, pool);

            var swEndPoint = (SimpleWebEndPoint)endPoint;
            server.Listen(swEndPoint.Port);
        }

        public void Close()
        {
            server?.Stop();
        }

        public void Connect(IEndPoint endPoint)
        {
            throw new NotSupportedException();
        }

        public bool Poll()
        {
            return server.receiveQueue.Count > 0;
        }

        public int Receive(byte[] buffer, out IEndPoint endPoint)
        {
            bool result = server.receiveQueue.TryDequeue(out Message message);
            if (!result)
            {
                throw new InvalidOperationException("No Packets in queue");
            }

            endPoint = ReceiveEndpoint;
            ReceiveEndpoint.ConnectionId = message.connId;

            switch (message.type)
            {
                case EventType.Connected:
                    {
                        // do nothing with connected event?
                        // Peer will do its own handshake using data

                        // use keep alive so this packet can just be thrown away
                        buffer[0] = (byte)PacketType.KeepAlive;
                        return 1;
                    }
                case EventType.Data:
                    {
                        message.data.CopyTo(buffer, 0);
                        int count = message.data.count;
                        message.data.Release();
                        return count;
                    }
                case EventType.Error:
                case EventType.Disconnected:
                    {
                        buffer[0] = (byte)PacketType.Command;
                        buffer[1] = (byte)Commands.Disconnect;
                        buffer[2] = (byte)(message.type == EventType.Disconnected ? SimpleWebSocketDisconnectReasons.SocketClosed : SimpleWebSocketDisconnectReasons.SocketError);
                        return 3;
                    }
            }

            // default, either endpoint not found or error.
            endPoint = null;
            return 0;
        }

        public void Send(IEndPoint endPoint, byte[] packet, int length)
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

        SimpleWebEndPoint ReceiveEndpoint;

        public ClientWebSocket(TcpConfig tcpConfig, int maxMessageSize, SslConfig sslConfig)
        {
            this.tcpConfig = tcpConfig;
            this.maxMessageSize = maxMessageSize;
            this.sslConfig = sslConfig;
        }

        public void Bind(IEndPoint endPoint)
        {
            throw new NotSupportedException();
        }

        public void Close()
        {
            client?.Disconnect();
            client = null;
        }

        public void Connect(IEndPoint endPoint)
        {
            client = SimpleWebClient.Create(maxMessageSize, 10_000, tcpConfig);

            ReceiveEndpoint = (SimpleWebEndPoint)endPoint;
            var builder = new UriBuilder
            {
                Scheme = GetClientScheme(),
                Host = ReceiveEndpoint.HostName,
                Port = ReceiveEndpoint.Port
            };

            client.Connect(builder.Uri);
        }
        string GetClientScheme() => (sslConfig.enabled) ? SecureScheme : NormalScheme;

        public bool Poll()
        {
            return client.receiveQueue.Count > 0;
        }

        public int Receive(byte[] buffer, out IEndPoint endPoint)
        {
            endPoint = ReceiveEndpoint;

            bool result = client.receiveQueue.TryDequeue(out Message next);
            if (!result)
            {
                throw new InvalidOperationException("No Packets in queue");
            }

            switch (next.type)
            {
                case EventType.Connected:
                    {
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

        public void Send(IEndPoint endPoint, byte[] packet, int length)
        {
            client.Send(new ArraySegment<byte>(packet, 0, length));
        }
    }
}
