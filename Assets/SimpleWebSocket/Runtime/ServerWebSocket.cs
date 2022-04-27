using System;
using JamesFrowen.SimpleWeb;
using Mirage.SocketLayer;
using EventType = JamesFrowen.SimpleWeb.EventType;

namespace JamesFrowen.Mirage.Sockets.SimpleWeb
{
    public class ServerWebSocket : ISocket
    {
        private readonly TcpConfig tcpConfig;
        private readonly int maxMessageSize;
        private readonly SslConfig sslConfig;
        const int handshakeMaxSize = 3000;

        private BufferPool pool;
        private WebSocketServer server;

        SimpleWebEndPoint ReceiveEndpoint = new SimpleWebEndPoint();

        public ServerWebSocket(TcpConfig tcpConfig, int maxMessageSize, SslConfig sslConfig)
        {
            this.tcpConfig = tcpConfig;
            this.maxMessageSize = maxMessageSize;
            this.sslConfig = sslConfig;

            pool = new BufferPool(5, 20, maxMessageSize);
            server = new WebSocketServer(tcpConfig, maxMessageSize, Math.Min(maxMessageSize, handshakeMaxSize), sslConfig, pool);
        }

        public void Bind(IEndPoint endPoint)
        {
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
}
