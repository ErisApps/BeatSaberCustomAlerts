using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatCore;
using ChatCore.Interfaces;
using ChatCore.Models.Twitch;
using ChatCore.Services;
using ChatCore.Services.Twitch;
using Streamlabs;
using Streamlabs.Utilities;
using UnityStreamlabs;
using System.Threading;

namespace Streamlabs
{
    class ChatService
    {
        private static ChatServiceMultiplexer streamingService;
        private static TwitchService twitchService;
        private static SynchronizationContext unitySyncContext;

        public static event Action<StreamlabsEvent> OnEvent;

        public static event Action<StreamlabsEvent> OnNormalEvent;

        private static string twitchChannel;
        public static void StartChatService()
        {
            twitchChannel = Plugin.config.GetString("Twitch", "Channel", "TWITCH_NAME");
            if (twitchChannel == "TWITCH_NAME")
            {
                Plugin.config.SetString("Twitch", "Channel", "TWITCH_NAME");
                return;
            }
            unitySyncContext = SynchronizationContext.Current;

            var streamCore = ChatCoreInstance.Create();
            streamingService = streamCore.RunAllServices();
            twitchService = streamingService.GetTwitchService();
            streamingService.OnLogin += StreamingService_OnLogin;
            streamingService.OnTextMessageReceived += StreamServiceProvider_OnMessageReceived;
            streamingService.OnJoinChannel += StreamServiceProvider_OnChannelJoined;
            streamingService.OnLeaveChannel += StreamServiceProvider_OnLeaveChannel;
            streamingService.OnRoomStateUpdated += StreamServiceProvider_OnChannelStateUpdated;
            //Console.WriteLine($"StreamService is of type {streamServiceProvider.ServiceType.Name}");
        }

        private static void StreamingService_OnLogin(IChatService svc)
        {
            if (svc is TwitchService twitchService)
            {
                twitchService.JoinChannel(twitchChannel);
            }
        }

        private static void StreamServiceProvider_OnChannelStateUpdated(IChatService svc, IChatChannel channel)
        {
            Console.WriteLine($"Channel state updated for {channel.GetType().Name} {channel.Id}");
            if (channel is TwitchChannel twitchChannel)
            {
                Console.WriteLine($"RoomId: {twitchChannel.Roomstate.RoomId}");
            }
        }

        private static void StreamServiceProvider_OnLeaveChannel(IChatService svc, IChatChannel channel)
        {
            Console.WriteLine($"Left channel {channel.Id}");
        }

        private static void StreamServiceProvider_OnChannelJoined(IChatService svc, IChatChannel channel)
        {
            Console.WriteLine($"Joined channel {channel.Id}");
        }

        private static void StreamServiceProvider_OnMessageReceived(IChatService svc, IChatMessage msg)
        {
            try
            {
                Console.WriteLine($"{msg.Sender.DisplayName}: {msg.Message}");
                Console.WriteLine("NAME:");
                Console.WriteLine(msg.Sender.DisplayName);
                string[] redeemString = msg.Message.Split(new string[] { "redeemed " }, StringSplitOptions.None);
                if (msg is TwitchMessage twitchMsg)
                {
                    int bits = twitchMsg.Bits;
                    if(bits > 0)
                    {
                        StreamlabsEvent streamlabsEvent = new StreamlabsEvent();
                        streamlabsEvent.type = "bits";
                        streamlabsEvent.message = new Message[1];
                        streamlabsEvent.message[0] = new Message();
                        streamlabsEvent.message[0].name = msg.Sender.UserName;
                        streamlabsEvent.message[0].amount = twitchMsg.Bits.ToString();
                        unitySyncContext.Send(SafeInvokeNormalStreamEvent, streamlabsEvent);
                    }
                }
                if (redeemString.Length > 1)
                {
                    Console.WriteLine(redeemString[1]);
                    Console.WriteLine(redeemString[0]);
                    Console.WriteLine("REDEEMED ^");
                    StreamlabsEvent streamlabsEvent = new StreamlabsEvent();
                    streamlabsEvent.type = "channelpoints";
                    streamlabsEvent.message = new Message[1];
                    streamlabsEvent.message[0] = new Message();
                    streamlabsEvent.message[0].name = redeemString[0].Split(new string[] { "] " }, StringSplitOptions.None)[1].Trim();
                    streamlabsEvent.message[0].channelPointsName = redeemString[1].Trim();
                    unitySyncContext.Send(SafeInvokeStreamEvent, streamlabsEvent);

                    /*foreach (CustomAlert alert in AlertObjectLoader.CustomAlerts)
                    {
                        if(alert.Descriptor.ChannelPointsName.ToLower().Trim() == redeemString[1].ToLower().Trim())
                        {
                            //StreamlabsEvent labsEvent = new StreamlabsEvent();
                            //labsEvent.type = "channelpoints";
                            //labsEvent.message[0] = new Message();
                            //labsEvent.message[0].name = redeemString[0].Trim();
                            //alert.streamEvent = labsEvent;
                            //Debug.WriteLine(redeemString[0]);
                            //alert.ChannelPointsUser = redeemString[0].Split(new string[] { "] " }, StringSplitOptions.None)[1].Trim();
                            //alert.Spawn
                        }
                    }*/
                    //.streamEvent = streamlabsEvent;

                }
                void SafeInvokeStreamEvent(object streamEvent) => OnEvent?.Invoke(streamEvent as StreamlabsEvent);
                void SafeInvokeNormalStreamEvent(object streamEvent) => OnNormalEvent?.Invoke(streamEvent as StreamlabsEvent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            //Console.WriteLine(msg.ToJson().ToString());
        }

        /*private void button1_Click(object sender, EventArgs e)
        {
            streamingService.GetTwitchService().PartChannel("xqcow");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            streamingService.GetTwitchService().JoinChannel("xqcow");
        }*/

    }
}
