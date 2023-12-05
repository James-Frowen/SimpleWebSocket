using System;
using System.Security.Authentication;
using JamesFrowen.Mirage.Sockets.SimpleWeb;
using JamesFrowen.SimpleWeb;
using Mirage.SocketLayer;
using UnityEngine;

namespace JamesFrowen.Mirage.Sockets.SimpleWeb
{
    public sealed class WebSocketFactory : SocketFactory, IHasAddress, IHasPort
    {
        string IHasAddress.Address { get => ClientUri; set => ClientUri = value; }
        int IHasPort.Port { get => ServerPort; set => ServerPort = value; }

        [Tooltip("Port for serer to listen on")]
        public int ServerPort = 7777;

        public ClientPortSettings ClientPort = new ClientPortSettings { Option = ClientPortOptions.SameAsServer };

        [Tooltip("Address client will use to connect. If using a reverse proxy, this should be the path and port for that. note, Scheme will be changed based on if SSL is being used")]
        public string ClientUri = "ws://localhost/path";

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
        [SerializeField] private Log.Levels _logLevels = Log.Levels.none;

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

        private void OnValidate()
        {
            Log.level = _logLevels;
        }

        private void Awake()
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
            return new BindEndPoint(ServerPort);
        }

        public override IEndPoint GetConnectEndPoint(string address = null, ushort? port = null)
        {
            UriBuilder builder;
            if (string.IsNullOrEmpty(address))
                builder = new UriBuilder(ClientUri);
            else
                builder = new UriBuilder(address);

            if (port.HasValue)
            {
                builder.Port = (int)port;
            }

            return new SimpleWebEndPoint(builder.Uri);
        }

        private static bool IsWebgl => Application.platform == RuntimePlatform.WebGLPlayer;
    }

    [Serializable]
    public struct ClientPortSettings
    {
        public ClientPortOptions Option;
        public ushort CustomPort;
    }
    public enum ClientPortOptions
    {
        SameAsServer,
        HttpDefault,
        Custom
    }
}
namespace Mirror.SimpleWeb.EditorScripts
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ClientPortSettings))]
    public class ClientPortSettingsDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // force exapnd
            property.isExpanded = true;

            var optionHeight = EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(ClientPortSettings.Option)));
            var portHeight = EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(ClientPortSettings.CustomPort)));
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            return optionHeight + spacing + portHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var optionProp = property.FindPropertyRelative(nameof(ClientPortSettings.Option));
            var portProp = property.FindPropertyRelative(nameof(ClientPortSettings.CustomPort));

            var optionHeight = EditorGUI.GetPropertyHeight(optionProp);
            var portHeight = EditorGUI.GetPropertyHeight(portProp);
            var spacing = EditorGUIUtility.standardVerticalSpacing;

            position.height = optionHeight;
            EditorGUI.PropertyField(position, optionProp);
            position.y += spacing + optionHeight;
            position.height = portHeight;

            var option = (ClientPortOptions)optionProp.enumValueIndex;
            if (option == ClientPortOptions.HttpDefault || option == ClientPortOptions.SameAsServer)
            {
                var port = 0;
                if (property.serializedObject.targetObject is WebSocketFactory swt)
                {
                    if (option == ClientPortOptions.HttpDefault)
                        port = swt.clientUseWss ? 443 : 80;
                    else
                        port = swt.ServerPort;
                }

                var wasEnabled = GUI.enabled;
                GUI.enabled = false;
                EditorGUI.IntField(position, new GUIContent("Client Port"), port);
                GUI.enabled = wasEnabled;
            }
            else
            {
                EditorGUI.PropertyField(position, portProp);
            }
        }
    }
#endif
}
