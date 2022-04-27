using System;
using System.Security.Authentication;
using JamesFrowen.SimpleWeb;
using Mirage.SocketLayer;
using UnityEngine;

namespace JamesFrowen.Mirage.Sockets.SimpleWeb
{
    public sealed class WebSocketFactory : SocketFactory, IHasAddress, IHasPort
    {
        string IHasAddress.Address { get => address; set => address = value; }
        int IHasPort.Port { get => port; set => port = value; }

        public string address = "localhost";
        public int port = 7777;
        public TcpConfig tcpConfig;

        [Tooltip("Note this sets Buffer size for socket layer, so larger numbers will require more memory.")]
        public int _maxPacketSize = 16384;

        [Header("Ssl Settings")]
        [Tooltip("Sets connect scheme to wss. Useful when client needs to connect using wss when TLS is outside of transport, NOTE: if sslEnabled is true clientUseWss is also true")]
        public bool clientUseWss;

        public bool sslEnabled;
        [Tooltip("Path to json file that contains path to cert and its password\n\nUse Json file so that cert password is not included in client builds\n\nSee cert.example.Json")]
        public string sslCertJson = "./cert.json";
        public SslProtocols sslProtocols = SslProtocols.Tls12;

        [Header("Debug")]
        [Tooltip("Log functions uses ConditionalAttribute which will effect which log methods are allowed. DEBUG allows warn/error, SIMPLEWEB_LOG_ENABLED allows all")]
        [SerializeField] Log.Levels _logLevels = Log.Levels.none;

        /// <summary>
        /// <para>Gets _logLevels field</para>
        /// <para>Sets _logLevels and Log.level fields</para>
        /// </summary>
        public Log.Levels LogLevels
        {
            get => _logLevels;
            set
            {
                _logLevels = value;
                Log.level = _logLevels;
            }
        }

        void OnValidate()
        {
            Log.level = _logLevels;
        }
        void Awake()
        {
            Log.level = _logLevels;
        }

        public override int MaxPacketSize => _maxPacketSize;

        public override ISocket CreateClientSocket()
        {
            // todo get max message size somewhere else?
            bool useWss = sslEnabled || clientUseWss;
            return new ClientWebSocket(tcpConfig, MaxPacketSize, useWss);
        }

        public override ISocket CreateServerSocket()
        {
            if (IsWebgl)
            {
                throw new NotSupportedException("Webgl can not be a server");
            }

            // todo get max message size somewhere else?
            SslConfig sslConfig = SslConfigLoader.Load(sslEnabled, sslCertJson, sslProtocols);
            return new ServerWebSocket(tcpConfig, MaxPacketSize, sslConfig);
        }

        public override IEndPoint GetBindEndPoint()
        {
            return new SimpleWebEndPoint(default, port);
        }

        public override IEndPoint GetConnectEndPoint(string address = null, ushort? port = null)
        {
            string addressString = address ?? this.address;
            ushort portIn = port ?? (ushort)this.port;

            return new SimpleWebEndPoint(addressString, portIn);
        }

        private static bool IsWebgl => Application.platform == RuntimePlatform.WebGLPlayer;
    }
}
