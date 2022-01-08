﻿using Zenject;
using UnityEngine;
using System.Collections;
using CustomAlerts.Models;
using CustomAlerts.Models.Events;
using CustomAlerts.Utilities;
using System.Collections.Generic;
using System.Linq;
using IPA.Utilities.Async;
using SiraUtil.Logging;

namespace CustomAlerts.Queuing
{
    internal class BasicQueueController : MonoBehaviour, IAlertQueue
    {
        private ChatService _chatService;
        private AlertObjectManager _alertObjectLoader;
        private readonly Queue<IAlert> _queuedAlerts = new();

        [Inject]
        public void Construct(SiraLog logger, ChatService chatService, AlertObjectManager alertObjectLoader)
        {
            _chatService = chatService;
            _alertObjectLoader = alertObjectLoader;

            _chatService.OnEvent += OnEvent;

            logger.Notice("Queue Controller Constructed");
        }

        public void OnDestroy()
        {
            _chatService.OnEvent -= OnEvent;
        }

        private void OnEvent(TwitchEvent twitchEvent)
        {
            CustomAlert alert = _alertObjectLoader.GetAlertByType(twitchEvent.AlertType, twitchEvent.AlertType == AlertType.ChannelPoints ? twitchEvent.Message.FirstOrDefault()?.ChannelPointsName : null);
            AlertData alertData = _alertObjectLoader.Process(alert, twitchEvent);
            if (alertData.canSpawn)
            {
                CustomAlert newAlert = new CustomAlert(alert.GameObject, alert.Descriptor, twitchEvent)
                {
                    Flatline = alertData.delay,
                    Volume = alertData.volume
                };
                
                Enqueue(newAlert);
            }
        }

        public void Enqueue(IAlert alert)
        {
            _queuedAlerts.Enqueue(alert);
            if (_queuedAlerts.Count == 1)
            {
                UnityMainThreadTaskScheduler.Factory.StartNew(() => StartCoroutine(PlayQueue()));
            }
        }

        private IEnumerator PlayQueue()
        {
            IAlert alert = _queuedAlerts.Peek();
            alert.Spawn();
            yield return new WaitForSecondsRealtime(alert.Lifeline);
            yield return new WaitForSecondsRealtime(alert.Flatline);
            _queuedAlerts.Dequeue();
            if (_queuedAlerts.Count != 0)
            {
                StartCoroutine(PlayQueue());
            }
        }
    }
}
