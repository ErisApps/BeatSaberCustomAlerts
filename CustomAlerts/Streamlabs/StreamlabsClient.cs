using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading;
using System.Reflection;
using ChatCore.Interfaces;
using System.Threading.Tasks;
using UnityEngine.Networking;
using CustomAlerts.Configuration;

namespace CustomAlerts.Streamlabs
{
    public class StreamlabsClient : IDisposable
    {
        public ConnectionState State { get; private set; }

        public event Action<StreamlabsEvent> OnEvent;
        public event Action<ConnectionState> OnConnectionStateChanged;

        private string _token;
        private readonly Configuration.PluginConfig _pluginConfig;
        private UnityWebRequest _donationRequest;
        private readonly IWebSocketService _webSocketService;
        private SynchronizationContext _synchronizationContext;
        private CancellationTokenSource _heartbeatCancellationToken;

        public StreamlabsClient(Configuration.PluginConfig pluginConfig, IWebSocketService webSocketService)
        {
            _pluginConfig = pluginConfig;
            _webSocketService = webSocketService;
            _webSocketService.OnOpen += OnOpen;
            _webSocketService.OnClose += OnClose;
            _webSocketService.OnError += OnError;
            Connect();

            Plugin.Log.Notice("Streamlabs Client Constructed");
        }

        public void Connect()
        {
            if (State != ConnectionState.NotConnected)
            {
                return;
            }
            _token = _pluginConfig.Tokens.SocketToken;
            if (_token == "PUT_TOKEN_HERE")
            {
                Application.OpenURL("https://streamlabs.com/api/v1.0/authorize?response_type=code&client_id=skX0cQaaRQLlPEaUi8eV6ZweLD8xKdz7Biegbd3g&redirect_uri=https://bs-customalerts-auth.herokuapp.com/oauth&scope=socket.token");
                return;
            }
            _synchronizationContext = SynchronizationContext.Current;
            ChangeConnectionState(ConnectionState.Connecting);
            StartupSocket();            
        }

        private void StartupSocket()
        {
            if (_webSocketService.IsConnected)
            {
                // Disconnect the socket before reconnecting.
                _webSocketService.OnMessageReceived -= OnSocketMessageReceived;    
                _webSocketService.Disconnect();
            }
            _webSocketService.OnMessageReceived += OnSocketMessageReceived;
            _webSocketService.Connect($"wss://sockets.streamlabs.com/socket.io/?token={_token}&EIO=3&transport=websocket");
        }

        public void Disconnect()
        {
            if (_donationRequest != null)
            {
                _donationRequest.Abort();
                _donationRequest.Dispose();
                _donationRequest = null;
            }
            OnClose();
        }

        private void OnSocketMessageReceived(Assembly assembly, string message)
        {
            try
            {
                var code = message[0];
                var data = message.Substring(1);

                if (code == '0') // socket.io ping-pong heartbeat
                {
                    _heartbeatCancellationToken?.Cancel();
                    _heartbeatCancellationToken = new CancellationTokenSource();
                    var interval = JsonConvert.DeserializeObject<HandshakeResponse>(data).PingTimeout;
                    WebSocketHeartbeatRoutine(interval, _heartbeatCancellationToken.Token);
                    return;
                }
                if (message.Contains("event"))
                {
                    string splitData = message.Split(new string[] { "\"event\"" }, StringSplitOptions.None)[1].Substring(1);
                    splitData = splitData.Remove(splitData.Length - 1);
                    StreamlabsEvent streamlabsEvent = JsonConvert.DeserializeObject<StreamlabsEvent>(splitData);
                    Plugin.Log.Info(splitData);
                    _synchronizationContext.Send(SafeInvokeStreamEvent, streamlabsEvent);
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Warn($"Streamlabs message handler failed! {e.Message}");
            }

            void SafeInvokeStreamEvent(object se) => OnEvent?.Invoke(se as StreamlabsEvent);
        }

        private void ChangeConnectionState(ConnectionState state)
        {
            if (State == state)
            {
                return;
            }
            State = state;

            // This method is called on a background thread; rerouting it to the Unity's thread.
            _synchronizationContext.Send(SafeInvoke, state);
            void SafeInvoke(object obj) => OnConnectionStateChanged?.Invoke((ConnectionState)obj);
        }

        private void OnOpen()
        {
            Plugin.Log.Notice("Streamlabs Socket Opening");
        }

        private void OnClose()
        {
            Plugin.Log.Notice("Streamlabs Socket Closing");
            ChangeConnectionState(ConnectionState.NotConnected);
        }

        private void OnError()
        {
            Plugin.Log.Error("An error has occured on the streamlabs client websocket.");
        }

        private async void WebSocketHeartbeatRoutine(int interval, CancellationToken cancellationToken)
        {
            while (_webSocketService.IsConnected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _webSocketService.SendMessage("2");
                }
                catch (Exception e)
                {
                    Plugin.Log.Warn($"Streamlabs socket heartbeat failed! {e.Message}");
                }

                await Task.Delay(interval);
            }
        }

        public void Dispose()
        {
            if (_webSocketService.IsConnected)
            {
                _webSocketService.Disconnect();
            }
            ChangeConnectionState(ConnectionState.NotConnected);

            _webSocketService.OnOpen -= OnOpen;
            _webSocketService.OnClose -= OnClose;
            _webSocketService.OnError -= OnError;
        }
    }
}