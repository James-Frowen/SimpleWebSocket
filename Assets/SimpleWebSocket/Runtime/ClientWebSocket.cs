using System;
using System.Collections.Concurrent;
using JamesFrowen.SimpleWeb;
using Mirage.SocketLayer;

namespace JamesFrowen.Mirage.Sockets.SimpleWeb
{
    public class ClientWebSocket : CommonWebSocket
    {
        public const string NormalScheme = "ws";
        public const string SecureScheme = "wss";

        private readonly TcpConfig tcpConfig;
        private readonly int maxMessageSize;
        private readonly bool useWss;
        private readonly bool allowSslErrors;
        private SimpleWebClient client;

        public ClientWebSocket(TcpConfig tcpConfig, int maxMessageSize, bool useWss, bool allowSslErrors)
        {
            this.tcpConfig = tcpConfig;
            this.maxMessageSize = maxMessageSize;
            this.useWss = useWss;
            this.allowSslErrors = allowSslErrors;
        }

        public override IConnectionHandle Connect(IConnectEndPoint _endPoint)
        {
            client = SimpleWebClient.Create(maxMessageSize, 10_000, tcpConfig, allowSslErrors);
            var endPoint = (ConnectEndPoint)_endPoint;
            var scheme = useWss ? SecureScheme : NormalScheme;
            var builder = new UriBuilder(scheme, endPoint.address, endPoint.port);
            client.Connect(builder.Uri);
            var handle = new SimpleWebConnectionHandle(MirageDisconnected, null);
            return handle;
        }

        public override void Close()
        {
            client?.Disconnect();
            client = null;
        }

        protected override ConcurrentQueue<Message> GetReceiveQueue()
        {
            return client.receiveQueue;
        }

        protected override void MirageDisconnected(SimpleWebConnectionHandle handle)
        {
            client.Disconnect();
        }

        public override void Send(IConnectionHandle _handle, ReadOnlySpan<byte> span)
        {
            client.Send(span);
        }
    }
}
