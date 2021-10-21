using System;
using System.Threading;
using CatCore;
using CatCore.Models.Shared;
using CatCore.Models.Twitch.IRC;
using CatCore.Models.Twitch.PubSub.Responses;
using CatCore.Models.Twitch.PubSub.Responses.ChannelPointsChannelV1;
using CatCore.Services.Interfaces;
using CatCore.Services.Twitch.Interfaces;
using CustomAlerts.Models.Events;
using SiraUtil.Tools;

namespace CustomAlerts
{
	internal class ChatService : IDisposable
	{
		private readonly SiraLog _logger;

		private readonly ChatCoreInstance _catCoreInstance;
		private readonly ITwitchService _twitchService;
		private readonly ITwitchPubSubServiceManager _twitchPubSubServiceManager;

		private readonly SynchronizationContext _synchronizationContext;

		public ChatService(SiraLog logger, ChatCoreInstance catCoreInstance)
		{
			_synchronizationContext = SynchronizationContext.Current;

			_logger = logger;
			_catCoreInstance = catCoreInstance;
			_twitchService = _catCoreInstance.RunTwitchServices();

			_twitchService.OnTextMessageReceived += TwitchService_OnMessageReceived;

			_twitchPubSubServiceManager = _twitchService.GetPubSubService();
			_twitchPubSubServiceManager.OnFollow += TwitchPubSub_OnFollow;
			_twitchPubSubServiceManager.OnRewardRedeemed += TwitchPubSub_OnRewardRedeemed;
		}

		public event Action<TwitchEvent> OnEvent;

		private void TwitchService_OnMessageReceived(IChatService chatService, IChatMessage chatMessage)
		{
			try
			{
				var twitchMessage = (TwitchMessage)chatMessage;
				if (twitchMessage.Bits > 0)
				{
					var twitchEvent = new TwitchEvent
					{
						AlertType = AlertType.Bits,
						Message = new[]
						{
							new Message
							{
								Name = chatMessage.Sender.UserName,
								Amount = twitchMessage.Bits.ToString()
							}
						}
					};

					_synchronizationContext.Send(SafeInvokeTwitchEvent, twitchEvent);

					return;
				}

				if (!twitchMessage.Metadata.TryGetValue(IrcMessageTags.MSG_ID, out var noticeType))
				{
					return;
				}

				switch (noticeType)
				{
					// TODO: What about subgifts and anon subs and such?
					case "sub":
					case "resub":
						_synchronizationContext.Send(SafeInvokeTwitchEvent, new TwitchEvent
						{
							AlertType = AlertType.Subscription,
							Message = new[]
							{
								new Message
								{
									Name = chatMessage.Sender.UserName
								}
							}
						});

						break;
					case "raid":
						var viewerCount = int.Parse(twitchMessage.Metadata[IrcMessageTags.MSG_PARAM_VIEWER_COUNT]);
						_synchronizationContext.Send(SafeInvokeTwitchEvent, new TwitchEvent
						{
							AlertType = AlertType.Raids,
							Message = new[]
							{
								new Message
								{
									Name = chatMessage.Sender.UserName,
									Viewers = viewerCount
								}
							}
						});

						break;
				}
				
				// TODO: Cover HOST
			}
			catch (Exception e)
			{
				_logger.Error($"Error when processing received chat message: {e.Message}");
			}
		}

		private void TwitchPubSub_OnFollow(string channelId, Follow followData)
		{
			var twitchEvent = new TwitchEvent
			{
				AlertType = AlertType.Follow,
				Message = new[]
				{
					new Message
					{
						Name = followData.DisplayName
					}
				}
			};
			_synchronizationContext.Send(SafeInvokeTwitchEvent, twitchEvent);
		}

		private void TwitchPubSub_OnRewardRedeemed(string channelId, RewardRedeemedData rewardRedeemedData)
		{
			var twitchEvent = new TwitchEvent
			{
				AlertType = AlertType.ChannelPoints,
				Message = new[]
				{
					new Message
					{
						Name = rewardRedeemedData.User.DisplayName,
						ChannelPointsName = rewardRedeemedData.Reward.Title
					}
				}
			};
			_synchronizationContext.Send(SafeInvokeTwitchEvent, twitchEvent);
		}

		private void SafeInvokeTwitchEvent(object twitchEventObj)
		{
			var twitchEvent = (TwitchEvent)twitchEventObj;
			_logger.Logger.Notice($"Handling TwitchEvent of type \"{twitchEvent.AlertType:G}\"");
			OnEvent?.Invoke(twitchEvent);
		}

		public void Dispose()
		{
			_twitchService.OnTextMessageReceived -= TwitchService_OnMessageReceived;

			_twitchPubSubServiceManager.OnFollow -= TwitchPubSub_OnFollow;
			_twitchPubSubServiceManager.OnRewardRedeemed -= TwitchPubSub_OnRewardRedeemed;

			_catCoreInstance.StopTwitchServices();
		}
	}
}