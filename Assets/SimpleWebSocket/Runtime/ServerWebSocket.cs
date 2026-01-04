using System;
using System.Collections.Concurrent;
using JamesFrowen.SimpleWeb;
using Mirage.SocketLayer;

namespace JamesFrowen.Mirage.Sockets.SimpleWeb
{

    public class ServerWebSocket : CommonWebSocket
    {
        private BufferPool pool;
        private WebSocketServer server;

        public ServerWebSocket(TcpConfig tcpConfig, int maxMessageSize, int handshakeMaxSize, SslConfig sslConfig)
        {
            pool = new BufferPool(5, 20, maxMessageSize);
            server = new WebSocketServer(tcpConfig, maxMessageSize, handshakeMaxSize, sslConfig, pool);
        }

        public override void Bind(IBindEndPoint _endPoint)
        {
            var endPoint = (BindEndPoint)_endPoint;
            server.Listen(endPoint.Port);
        }

        public override void Close()
        {
            server?.Stop();
            server = null;
        }

        protected override ConcurrentQueue<Message> GetReceiveQueue()
        {
            return server.receiveQueue;
        }

        protected override void MirageDisconnected(SimpleWebConnectionHandle handle)
        {
            server.CloseConnection(handle.ServerConnection);
        }

        public override void Send(IConnectionHandle _handle, ReadOnlySpan<byte> packet)
        {
            var handle = (SimpleWebConnectionHandle)_handle;

            var buffer = pool.Take(packet.Length);
            buffer.CopyFrom(packet);
            server.Send(handle.ServerConnection, buffer);
        }
    }
}
