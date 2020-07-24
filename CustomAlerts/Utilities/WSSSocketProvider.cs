using System;
using WebSocketSharp;
using System.Reflection;
using ChatCore.Interfaces;

namespace CustomAlerts.Utilities
{
    public class WSSSocketProvider : IWebSocketService
    {
        public bool IsConnected => _webSocket != null && _webSocket.IsAlive;
        public bool AutoReconnect { get; set; }
        public int ReconnectDelay { get; set; }

        public event Action OnOpen;
        public event Action OnClose;
        public event Action OnError;
        public event Action<Assembly, string> OnMessageReceived;

        private WebSocket _webSocket;

        public void Connect(string uri, bool forceReconnect = false)
        {
            _webSocket = new WebSocket(uri)
            {
                EmitOnPing = true
            };
            _webSocket.OnOpen += HandleOpen;
            _webSocket.OnClose += HandleClose;
            _webSocket.OnError += HandleError;
            _webSocket.OnMessage += HandleSocketMessage;
            _webSocket.Connect();
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                _webSocket.Close();
                _webSocket.OnOpen -= HandleOpen;
                _webSocket.OnClose -= HandleClose;
                _webSocket.OnError -= HandleError;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        public void SendMessage(string message)
        {
            _webSocket.Send(message);
        }

        public void HandleOpen(object sender, EventArgs evt)
        {
            OnOpen?.Invoke();
        }

        public void HandleClose(object sender, CloseEventArgs evt)
        {
            OnClose?.Invoke();
            _webSocket.Close();
        }

        public void HandleError(object sender, ErrorEventArgs evt)
        {
            OnError?.Invoke();
        }

        private void HandleSocketMessage(object sender, MessageEventArgs evt)
        {
            OnMessageReceived?.Invoke(Assembly.GetExecutingAssembly(), evt.Data);
        }
    }
}
