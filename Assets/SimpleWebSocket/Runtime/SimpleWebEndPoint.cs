using System;
using Mirage.SocketLayer;

namespace JamesFrowen.Mirage.Sockets.SimpleWeb
{
    public class SimpleWebConnectionHandle : IConnectionHandle
    {
        public readonly JamesFrowen.SimpleWeb.IConnection ServerConnection;
        private readonly Action<SimpleWebConnectionHandle> DisconnectCallback;

        public SimpleWebConnectionHandle(Action<SimpleWebConnectionHandle> disconnectCallback, JamesFrowen.SimpleWeb.IConnection connection)
        {
            DisconnectCallback = disconnectCallback;
            ServerConnection = connection;
        }

        public override bool Equals(object obj)
        {
            // each connection should have its own c# object
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return ServerConnection?.Id ?? base.GetHashCode();
        }

        public override string ToString()
        {
            return ServerConnection != null
                ? $"Active connection:{ServerConnection.Id}"
                : $"Active connection:Client";
        }

        IConnectionHandle IConnectionHandle.CreateCopy() => throw new NotSupportedException("Create copy should not be called for Stateful connections");

        bool IConnectionHandle.IsStateful => true;
        ISocketLayerConnection IConnectionHandle.SocketLayerConnection { get; set; }
        bool IConnectionHandle.SupportsGracefulDisconnect => false;
        void IConnectionHandle.Disconnect(string gracefulDisconnectReason)
        {
            DisconnectCallback?.Invoke(this);
        }
    }
}
