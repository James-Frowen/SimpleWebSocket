using Mirage.SocketLayer;

namespace JamesFrowen.Mirage.Sockets.SimpleWeb
{
    public class BindEndPoint : IBindEndPoint
    {
        public readonly int Port;

        public BindEndPoint(int port)
        {
            Port = port;
        }
    }
}
