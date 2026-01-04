using Mirage.SocketLayer;

namespace JamesFrowen.Mirage.Sockets.SimpleWeb
{
    public class ConnectEndPoint : IConnectEndPoint
    {
        public readonly string address;
        public readonly int port;

        public ConnectEndPoint(string address, int port)
        {
            this.address = address;
            this.port = port;
        }

        public override string ToString()
        {
            return $"{address}:{port}";
        }
    }
}
