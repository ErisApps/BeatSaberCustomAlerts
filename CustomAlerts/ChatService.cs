using System;
using Zenject;
using ChatCore;
using IPA.Utilities;
using System.Threading;
using ChatCore.Services;
using ChatCore.Interfaces;
using ChatCore.Models.Twitch;
using CustomAlerts.Streamlabs;
using ChatCore.Services.Twitch;
using CustomAlerts.Configuration;

namespace CustomAlerts
{
    public class ChatService : IDisposable
    {
        public event Action<StreamlabsEvent> OnEvent;
        public event Action<StreamlabsEvent> OnNormalEvent;

        private readonly Config _config;
        private readonly string _twitchChannel;
        private readonly ChatServiceMultiplexer _streamingService;
        private readonly SynchronizationContext _synchronizationContext;

        public ChatService(Config config, ChatServiceMultiplexer streamingService)
        {
            Plugin.Log.Error("ello");
            Plugin.Log.Error("ello");
            Plugin.Log.Error("ello");
            Plugin.Log.Error("ello");
            _config = config;
            _streamingService = streamingService;
            _twitchChannel = _config.Twitch.Channel;
            if (_twitchChannel == "TWITCH_NAME")
            {
                return;
            }
            _synchronizationContext = SynchronizationContext.Current;
            _streamingService.OnLogin += StreamingService_OnLogin;
            _streamingService.OnTextMessageReceived += StreamServiceProvider_OnMessageReceived;
            _streamingService.OnJoinChannel += StreamServiceProvider_OnChannelJoined;
            _streamingService.OnLeaveChannel += StreamServiceProvider_OnLeaveChannel;
            _streamingService.OnRoomStateUpdated += StreamServiceProvider_OnChannelStateUpdated;
        }

        private void StreamingService_OnLogin(IChatService svc)
        {
            if (svc is TwitchService twitchService)
            {
                twitchService.JoinChannel(_twitchChannel);
            }
        }

        private void StreamServiceProvider_OnChannelStateUpdated(IChatService svc, IChatChannel channel)
        {
            
        }

        private void StreamServiceProvider_OnLeaveChannel(IChatService svc, IChatChannel channel)
        {
            
        }

        private void StreamServiceProvider_OnChannelJoined(IChatService svc, IChatChannel channel)
        {
            
        }

        private void StreamServiceProvider_OnMessageReceived(IChatService svc, IChatMessage msg)
        {
            try
            {
                string[] redeemString = msg.Message.Split(new string[] { "redeemed " }, StringSplitOptions.None);
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
                        Name = redeemString[0].Split(new string[] { "] " }, StringSplitOptions.None)[1].Trim(),
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
            _streamingService.OnLogin -= StreamingService_OnLogin;
            _streamingService.OnTextMessageReceived -= StreamServiceProvider_OnMessageReceived;
            _streamingService.OnJoinChannel -= StreamServiceProvider_OnChannelJoined;
            _streamingService.OnLeaveChannel -= StreamServiceProvider_OnLeaveChannel;
            _streamingService.OnRoomStateUpdated -= StreamServiceProvider_OnChannelStateUpdated;
        }
    }
}