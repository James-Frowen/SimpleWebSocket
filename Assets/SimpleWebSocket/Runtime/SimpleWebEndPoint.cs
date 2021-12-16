using Mirage.SocketLayer;

namespace JamesFrowen.Mirage.Sockets.SimpleWeb
{
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
}
