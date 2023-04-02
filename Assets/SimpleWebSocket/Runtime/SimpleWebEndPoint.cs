using System;
using Mirage.SocketLayer;

namespace JamesFrowen.Mirage.Sockets.SimpleWeb
{
    public class SimpleWebEndPoint : IEndPoint
    {
        internal int ConnectionId;

        public readonly Uri Uri;
        public readonly int ServerPort;

        public SimpleWebEndPoint(Uri uri)
        {
            Uri = uri;
        }
        internal SimpleWebEndPoint() { }
        private SimpleWebEndPoint(SimpleWebEndPoint other)
        {
            Uri = other.Uri;
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

                    return Uri.Equals(other.Uri);
                }


                return false;
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (ConnectionId != 0)
                return ConnectionId;

            return Uri.GetHashCode();
        }

        public override string ToString()
        {
            if (ConnectionId != 0)
                return $"Active connection:{ConnectionId}";
            else
                return $"{Uri}";
        }

        IEndPoint IEndPoint.CreateCopy()
        {
            return new SimpleWebEndPoint(this);
        }
    }
}
