using System;
using Mirage.SocketLayer;

namespace JamesFrowen.Mirage.Sockets.SimpleWeb
{
    public class BindEndPoint : IEndPoint
    {
        public readonly int Port;

        public BindEndPoint(int port)
        {
            Port = port;
        }

        public IEndPoint CreateCopy()
        {
            throw new NotImplementedException();
        }
    }
}
