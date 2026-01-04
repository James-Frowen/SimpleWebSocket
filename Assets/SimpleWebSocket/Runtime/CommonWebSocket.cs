using System;
using System.Collections.Concurrent;
using JamesFrowen.SimpleWeb;
using Mirage.SocketLayer;

namespace JamesFrowen.Mirage.Sockets.SimpleWeb
{
    public abstract class CommonWebSocket : ISocket
    {
        private OnData onData;
        private OnDisconnect onDisconnect;

        public virtual void Bind(IBindEndPoint endPoint) => throw new NotSupportedException();
        public abstract void Close();
        public virtual IConnectionHandle Connect(IConnectEndPoint endPoint) => throw new NotSupportedException();

        public void Flush() { }
        public bool Poll() => false;
        public int Receive(Span<byte> outBuffer, out IConnectionHandle handle) => throw new NotSupportedException();

        public abstract void Send(IConnectionHandle handle, ReadOnlySpan<byte> packet);
        public void SetTickEvents(int maxPacketSize, OnData onData, OnDisconnect onDisconnect)
        {
            this.onData = onData;
            this.onDisconnect = onDisconnect;
        }
        public void Tick()
        {
            var queue = GetReceiveQueue();
            while (queue.TryDequeue(out var message))
            {
                SimpleWebConnectionHandle handle;
                var context = message.conn;
                if (context == null)
                {
                    // new connection
                    handle = new SimpleWebConnectionHandle(MirageDisconnected, message.conn);
                    message.conn.Context = handle;
                }
                else
                {
                    handle = (SimpleWebConnectionHandle)message.conn.Context;
                }

                switch (message.type)
                {
                    case EventType.Connected:
                        {
                            // do nothing with connected event?
                            // Peer will do its own handshake using data
                            break;
                        }
                    case EventType.Data:
                        {
                            var data = message.data;
                            var span = new ReadOnlySpan<byte>(data.array, 0, data.count);
                            onData.Invoke(handle, span);
                            data.Release();
                            break;
                        }
                    case EventType.Error:
                    case EventType.Disconnected:
                        {
                            Span<byte> span = stackalloc byte[3];
                            span[0] = (byte)PacketType.Command;
                            span[1] = (byte)Commands.Disconnect;
                            span[2] = (byte)(message.type == EventType.Disconnected ? SimpleWebSocketDisconnectReasons.SocketClosed : SimpleWebSocketDisconnectReasons.SocketError);
                            onDisconnect.Invoke(handle, span, null);
                            break;
                        }
                }
            }
        }

        protected abstract ConcurrentQueue<Message> GetReceiveQueue();
        protected abstract void MirageDisconnected(SimpleWebConnectionHandle handle);
    }
}
