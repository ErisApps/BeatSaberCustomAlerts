using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using WebSocketSharp;
using Newtonsoft.Json;
using Streamlabs;
using Streamlabs.Utilities;
using System.Net;
using UnityEngine.Assertions.Must;

namespace UnityStreamlabs
{
    public class Message
    {
        public string name { get; set; }
        public string isTest { get; set; }
        public string _id { get; set; }
        public string priority { get; set; }
        public string amount { get; set; }
        public int raiders { get; set; } = 0;
        public int viewers { get; set; } = 0;
        public string channelPointsName { get; set; }
    }
    public class StreamlabsEvent
    {
        public string type { get; set; }
        public Message[] message { get; set; }
        public string eventFor { get; set; }
        public string eventId { get; set; }

        public AlertDescriptor.AlertType AlertType { 
            get{
                switch (type)
                {
                    case "channelpoints":
                        return AlertDescriptor.AlertType.ChannelPoints;
                    case "follow":
                        return AlertDescriptor.AlertType.Follow;
                    case "subscription":
                        return AlertDescriptor.AlertType.Subscription;
                    case "bits":
                        return AlertDescriptor.AlertType.Bits;
                    case "raid":
                        return AlertDescriptor.AlertType.Raids;
                    case "host":
                        return AlertDescriptor.AlertType.Host;
                    default:
                        return AlertDescriptor.AlertType.Other;
                }
            } 
        }

    }

    public static class Streamlabs
    {
        #pragma warning disable CS0649
        [Serializable]
        private struct HandshakeResponse { public int pingTimeout; }
        #pragma warning restore CS0649

        /// <summary>
        /// Invoked when <see cref="ConnectionState"/> is changed.
        /// </summary>
        public static event Action<ConnectionState> OnConnectionStateChanged;
        /// <summary>
        /// Invoked when a donation is sent.
        /// </summary>

        public static event Action<StreamlabsEvent> OnEvent;

        /// <summary>
        /// Current connection state to the Streamlabs server.
        /// </summary>
        public static ConnectionState ConnectionState { get; private set; }

        private static SynchronizationContext unitySyncContext;
        private static UnityWebRequest donationRequest;
        private static WebSocket webSocket;
        private static CancellationTokenSource heartbeatTCS;
        private static string token;
        /// <summary>
        /// Connect to the Streamlabs server to begin sending and receiving events.
        /// The connection process is async; listen for <see cref="OnConnectionStateChanged"/> to handle the result.
        /// </summary>
        public static void Connect ()
        {
            /*WebRequest request = WebRequest.Create(
              "https://bs-customalerts-auth.herokuapp.com/oauth");
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            Console.WriteLine(((HttpWebResponse)response));*/

            if (ConnectionState == ConnectionState.Connected || ConnectionState == ConnectionState.Connecting) return;
            token = Plugin.config.GetString("Tokens", "SocketToken", "PUT_TOKEN_HERE");
            if (token == "PUT_TOKEN_HERE")
            {
                Plugin.config.SetString("Tokens", "SocketToken", "PUT_TOKEN_HERE");
                Application.OpenURL("https://streamlabs.com/api/v1.0/authorize?response_type=code&client_id=skX0cQaaRQLlPEaUi8eV6ZweLD8xKdz7Biegbd3g&redirect_uri=https://bs-customalerts-auth.herokuapp.com/oauth&scope=socket.token");
                return;
            }
            //settings = StreamlabsSettings.LoadFromResources();
            unitySyncContext = SynchronizationContext.Current;
            ChangeConnectionState(ConnectionState.Connecting);
            InitializeWebSocket();
            //AuthController.OnAccessTokenRefreshed += HandleAccessTokenRefreshed;
            //AuthController.RefreshAccessToken();

            //void HandleAccessTokenRefreshed (bool success)
            //{
            //AuthController.OnAccessTokenRefreshed -= HandleAccessTokenRefreshed;
            //}
        }

        /// <summary>
        /// Disconnect from the Streamlabs server and stop receiving events.
        /// </summary>
        public static void Disconnect ()
        {
            if (donationRequest != null)
            {
                donationRequest.Abort();
                donationRequest.Dispose();
                donationRequest = null;
            }

            if (webSocket != null)
                webSocket.Close();

            ChangeConnectionState(ConnectionState.NotConnected);
        }

        /// <summary>
        /// Sends a test donation event.
        /// </summary>
        /// <param name="name">The name of the donor. has to be between 2-25 chars and can only be alphanumeric + underscores.</param>
        /// <param name="message">The message from the donor. must be < 255 characters.</param>
        /// <param name="identifier">An identifier for this donor, which is used to group donations with the same donor. For example, if you create more than one donation with the same identifier, they will be grouped together as if they came from the same donor. Typically this is best suited as an email address, or a unique hash.</param>
        /// <param name="amount">The amount of this donation.</param>
        /// <param name="currency">The 3 letter currency code for this donation. Must be one of the supported currency codes.</param>
        
        private static void InitializeWebSocket ()
        {
            webSocket = new WebSocket($"wss://sockets.streamlabs.com/socket.io/?token={token}&EIO=3&transport=websocket");
            webSocket.EmitOnPing = true;
            webSocket.OnOpen += HandleOpen;
            webSocket.OnClose += HandleClose;
            webSocket.OnError += HandleError;
            webSocket.OnMessage += HandleSocketMessage;
            webSocket.Connect();

            void HandleOpen (object sender, EventArgs evt)
            {
                //if (settings.EmitDebugMessages)
                    Debug.Log("WebSocket: Open");
                ChangeConnectionState(ConnectionState.Connected);
            }

            void HandleClose (object sender, CloseEventArgs evt)
            {
                //if (settings.EmitDebugMessages)
                    Debug.Log($"WebSocket: Close {evt.Code} {evt.Reason}");
                heartbeatTCS?.Cancel();
                ChangeConnectionState(ConnectionState.NotConnected);
            }

            void HandleError (object sender, ErrorEventArgs evt)
            {
                Debug.LogError($"Streamlabs web socket error: {evt.Exception} {evt.Message}");
            }
        }
        
        //2["event",{"type":"follow","message":[{"name":"bobbievr","isTest":true,"_id":"3db6e911084bf89505e1118023e985c9","priority":10}],"for":"twitch_account","event_id":"evt_fb97a212e48fb866cacd1cf6c5d24d4f"}]

        private static void HandleSocketMessage (object sender, MessageEventArgs evt)
        {
            try
            {
                //if (settings.EmitDebugMessages)
                Debug.Log("Message: " + evt.Data);

                var code = evt.Data[0];
                var data = evt.Data.Substring(1);

                if (code == '0') // socket.io ping-pong heartbeat
                {
                    heartbeatTCS?.Cancel();
                    heartbeatTCS = new CancellationTokenSource();
                    var interval = JsonUtility.FromJson<HandshakeResponse>(data).pingTimeout;
                    WebSocketHeartbeatRoutine(interval, heartbeatTCS.Token);
                    return;
                }
                if (evt.Data.Contains("event"))
                {
                    string splitData = evt.Data.Split(new string[] { "\"event\"" }, StringSplitOptions.None)[1].Substring(1);
                    splitData = splitData.Remove(splitData.Length - 1);
                    Console.WriteLine(splitData);
                    StreamlabsEvent streamlabsEvent = JsonConvert.DeserializeObject<StreamlabsEvent>(splitData);
                    unitySyncContext.Send(SafeInvokeStreamEvent, streamlabsEvent);
                }
                Console.WriteLine(data);
            }
            catch (Exception e) { UnityEngine.Debug.LogWarning($"WebSocket handle message fail: {e.Message}"); }

            void SafeInvokeStreamEvent(object streamEvent) => OnEvent?.Invoke(streamEvent as StreamlabsEvent);
        }

        private static void ChangeConnectionState (ConnectionState state)
        {
            if (ConnectionState == state) return;

            ConnectionState = state;

            // This method is called on a background thread; rerouting it to the Unity's thread.
            unitySyncContext.Send(SafeInoke, state);
            void SafeInoke (object obj) => OnConnectionStateChanged?.Invoke((ConnectionState)obj);
        }

        private static async void WebSocketHeartbeatRoutine (int interval, CancellationToken cancellationToken)
        {
            while (webSocket != null && webSocket.IsAlive && !cancellationToken.IsCancellationRequested)
            {
                try { webSocket.SendAsync("2", null); }
                catch (Exception e) { UnityEngine.Debug.LogWarning($"WebSocket heartbeat fail: {e.Message}"); }

                await Task.Delay(interval);
            }
        }
    }
}