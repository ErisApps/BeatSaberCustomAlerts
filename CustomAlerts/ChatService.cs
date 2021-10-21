using System;
using System.Threading;
using ChatCore.Services;
using ChatCore.Interfaces;
using ChatCore.Models.Twitch;
using CustomAlerts.Streamlabs;
using CustomAlerts.Configuration;

namespace CustomAlerts
{
    internal class ChatService : IDisposable
    {
        public event Action<StreamlabsEvent> OnEvent;
        public event Action<StreamlabsEvent> OnNormalEvent;

        private readonly PluginConfig _config;
        private readonly string _twitchChannel;
        private readonly ChatServiceMultiplexer _streamingService;
        private readonly SynchronizationContext _synchronizationContext;

        public ChatService(PluginConfig config, ChatServiceMultiplexer streamingService)
        {
            _config = config;
            _streamingService = streamingService;
            
            _synchronizationContext = SynchronizationContext.Current;
            _streamingService.OnTextMessageReceived += StreamServiceProvider_OnMessageReceived;
        }

        private void StreamServiceProvider_OnMessageReceived(IChatService svc, IChatMessage msg)
        {
            try
            {
                string[] redeemString = msg.Message.Split(new[] { "redeemed " }, StringSplitOptions.None);
                if (msg is TwitchMessage twitchMsg)
                {
                    int bits = twitchMsg.Bits;
                    if (bits > 0)
                    {
                        StreamlabsEvent streamlabsEvent = new StreamlabsEvent
                        {
                            Type = "bits",
                            Message = new Message[1]
                        };
                        streamlabsEvent.Message[0] = new Message
                        {
                            Name = msg.Sender.UserName,
                            Amount = twitchMsg.Bits.ToString()
                        };
                        _synchronizationContext.Send(SafeInvokeNormalStreamEvent, streamlabsEvent);
                    }
                }
                if (redeemString.Length > 1)
                {
                    StreamlabsEvent streamlabsEvent = new StreamlabsEvent
                    {
                        Type = "channelpoints",
                        Message = new Message[1]
                    };
                    streamlabsEvent.Message[0] = new Message
                    {
                        Name = redeemString[0].Split(new[] { "] " }, StringSplitOptions.None)[1].Trim(),
                        ChannelPointsName = redeemString[1].Trim()
                    };
                    _synchronizationContext.Send(SafeInvokeStreamEvent, streamlabsEvent);
                }
                void SafeInvokeStreamEvent(object streamEvent) => OnEvent?.Invoke(streamEvent as StreamlabsEvent);
                void SafeInvokeNormalStreamEvent(object streamEvent) => OnNormalEvent?.Invoke(streamEvent as StreamlabsEvent);
            }
            catch (Exception e)
            {
                Plugin.Log.Error($"Error when processing received chat message: {e.Message}");
            }
        }

        public void Dispose()
        {
            _streamingService.OnTextMessageReceived -= StreamServiceProvider_OnMessageReceived;
        }
    }
}