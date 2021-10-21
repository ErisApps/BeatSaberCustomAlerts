using System;
using System.Threading;
using ChatCore.Services;
using ChatCore.Interfaces;
using ChatCore.Models.Twitch;
using CustomAlerts.Streamlabs;

namespace CustomAlerts
{
    internal class ChatService : IDisposable
    {
        public event Action<TwitchEvent> OnEvent;
        public event Action<TwitchEvent> OnNormalEvent;

        private readonly string _twitchChannel;
        private readonly ChatServiceMultiplexer _streamingService;
        private readonly SynchronizationContext _synchronizationContext;

        public ChatService(ChatServiceMultiplexer streamingService)
        {
            _streamingService = streamingService;
            
            _synchronizationContext = SynchronizationContext.Current;
            _streamingService.OnTextMessageReceived += StreamServiceProvider_OnMessageReceived;
        }

        private void StreamServiceProvider_OnMessageReceived(IChatService svc, IChatMessage msg)
        {
            try
            {
                if (msg is TwitchMessage twitchMsg)
                {
                    int bits = twitchMsg.Bits;
                    if (bits > 0)
                    {
                        TwitchEvent twitchEvent = new TwitchEvent
                        {
                            AlertType = AlertType.Bits,
                            Message = new []
                            {
                                new Message
                                {
                                    Name = msg.Sender.UserName,
                                    Amount = twitchMsg.Bits.ToString()
                                }
                            }
                        };
                        _synchronizationContext.Send(SafeInvokeNormalStreamEvent, twitchEvent);
                    }
                }

                string[] redeemString = msg.Message.Split(new[] { "redeemed " }, StringSplitOptions.None);
                if (redeemString.Length > 1)
                {
                    TwitchEvent twitchEvent = new TwitchEvent
                    {
                        AlertType = AlertType.ChannelPoints,
                        Message = new []
                        {
                            new Message
                            {
                                Name = redeemString[0].Split(new[] { "] " }, StringSplitOptions.None)[1].Trim(),
                                ChannelPointsName = redeemString[1].Trim()
                            }
                        }
                    };
                    _synchronizationContext.Send(SafeInvokeStreamEvent, twitchEvent);
                }
                void SafeInvokeStreamEvent(object streamEvent) => OnEvent?.Invoke(streamEvent as TwitchEvent);
                void SafeInvokeNormalStreamEvent(object streamEvent) => OnNormalEvent?.Invoke(streamEvent as TwitchEvent);
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