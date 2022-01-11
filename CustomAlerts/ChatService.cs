using System;
using System.Threading.Tasks;
using CatCore;
using CatCore.Models.Twitch.IRC;
using CatCore.Models.Twitch.PubSub.Responses;
using CatCore.Models.Twitch.PubSub.Responses.ChannelPointsChannelV1;
using CatCore.Services.Twitch.Interfaces;
using CustomAlerts.Models.Events;
using SiraUtil.Logging;

namespace CustomAlerts
{
	internal class ChatService : IDisposable
	{
		private readonly SiraLog _logger;

		private readonly CatCoreInstance _catCoreInstance;
		private readonly ITwitchService _twitchService;
		private readonly ITwitchPubSubServiceManager _twitchPubSubServiceManager;

		public ChatService(SiraLog logger, CatCoreInstance catCoreInstance)
		{
			_logger = logger;
			_catCoreInstance = catCoreInstance;
			_twitchService = _catCoreInstance.RunTwitchServices();

			_twitchService.OnTextMessageReceived += TwitchService_OnMessageReceived;

			_twitchPubSubServiceManager = _twitchService.GetPubSubService();
			_twitchPubSubServiceManager.OnFollow += TwitchPubSub_OnFollow;
			_twitchPubSubServiceManager.OnRewardRedeemed += TwitchPubSub_OnRewardRedeemed;
		}

		public event Action<TwitchEvent> OnEvent;

		private void TwitchService_OnMessageReceived(ITwitchService chatService, TwitchMessage twitchMessage)
		{
			_ = Task.Run(() =>
			{
				try
				{
					if (twitchMessage.Bits > 0)
					{
						InvokeTwitchEvent(new TwitchEvent
						{
							AlertType = AlertType.Bits,
							Message = new Message
							{
								Name = twitchMessage.Sender.UserName,
								Amount = twitchMessage.Bits.ToString()
							}
						});

						return;
					}

					if (twitchMessage.Metadata == null || !twitchMessage.Metadata.TryGetValue(IrcMessageTags.MSG_ID, out var noticeType))
					{
						return;
					}

					switch (noticeType)
					{
						// TODO: What about subgifts and anon subs and such?
						case "sub":
						case "resub":
							var subscriberName = twitchMessage.Metadata[IrcMessageTags.DISPLAY_NAME];
							InvokeTwitchEvent(new TwitchEvent
							{
								AlertType = AlertType.Subscription,
								Message = new Message
								{
									Name = subscriberName
								}
							});

							break;
						case "raid":
							var raider = twitchMessage.Metadata[IrcMessageTags.MSG_PARAM_DISPLAY_NAME];
							var viewerCount = int.Parse(twitchMessage.Metadata[IrcMessageTags.MSG_PARAM_VIEWER_COUNT]);

							InvokeTwitchEvent(new TwitchEvent
							{
								AlertType = AlertType.Raids,
								Message = new Message
								{
									Name = raider,
									Viewers = viewerCount
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
			});
		}

		private void TwitchPubSub_OnFollow(string channelId, Follow followData)
		{
			_ = Task.Run(() =>
			{
				var twitchEvent = new TwitchEvent
				{
					AlertType = AlertType.Follow,
					Message = new Message
					{
						Name = followData.DisplayName
					}
				};
				InvokeTwitchEvent(twitchEvent);
			});
		}

		private void TwitchPubSub_OnRewardRedeemed(string channelId, RewardRedeemedData rewardRedeemedData)
		{
			_ = Task.Run(() =>
			{
				var twitchEvent = new TwitchEvent
				{
					AlertType = AlertType.ChannelPoints,
					Message = new Message
					{
						Name = rewardRedeemedData.User.DisplayName,
						ChannelPointsName = rewardRedeemedData.Reward.Title
					}
				};
				InvokeTwitchEvent(twitchEvent);
			});
		}

		private void InvokeTwitchEvent(TwitchEvent twitchEvent)
		{
			_logger.Notice($"Handling TwitchEvent of type \"{twitchEvent.AlertType:G}\"");

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