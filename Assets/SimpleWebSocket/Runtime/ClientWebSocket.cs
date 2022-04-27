using System;
using JamesFrowen.SimpleWeb;
using Mirage.SocketLayer;
using EventType = JamesFrowen.SimpleWeb.EventType;

namespace JamesFrowen.Mirage.Sockets.SimpleWeb
{
    public class ClientWebSocket : ISocket
    {
        public const string NormalScheme = "ws";
        public const string SecureScheme = "wss";

        private readonly TcpConfig tcpConfig;
        private readonly int maxMessageSize;
        private readonly bool useWss;

        private SimpleWebClient client;

        SimpleWebEndPoint ReceiveEndpoint;

        public ClientWebSocket(TcpConfig tcpConfig, int maxMessageSize, bool useWss)
        {
            this.tcpConfig = tcpConfig;
            this.maxMessageSize = maxMessageSize;
            this.useWss = useWss;
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
        string GetClientScheme() => useWss ? SecureScheme : NormalScheme;

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
