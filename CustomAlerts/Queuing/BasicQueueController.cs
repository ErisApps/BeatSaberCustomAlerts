using Zenject;
using UnityEngine;
using System.Threading;
using System.Collections;
using CustomAlerts.Models;
using CustomAlerts.Utilities;
using CustomAlerts.Streamlabs;
using System.Collections.Generic;

namespace CustomAlerts.Queuing
{
    internal class BasicQueueController : MonoBehaviour, IAlertQueue
    {
        private ChatService _chatService;
        private AlertObjectManager _alertObjectLoader;
        private SynchronizationContext _synchronizationContext;
        private readonly Queue<IAlert> _queuedAlerts = new Queue<IAlert>();

        [Inject]
        public void Construct(ChatService chatService, AlertObjectManager alertObjectLoader)
        {
            _chatService = chatService;
            _alertObjectLoader = alertObjectLoader;
            _synchronizationContext = SynchronizationContext.Current;

            _chatService.OnEvent += OnEvent;

            Plugin.Log.Notice("Queue Controller Constructed");
        }

        public void OnDestroy()
        {
            _chatService.OnEvent -= OnEvent;
        }

        private void OnEvent(TwitchEvent twitchEvent)
        {
            CustomAlert alert = _alertObjectLoader.GetAlertByType(twitchEvent.AlertType, twitchEvent.AlertType == AlertType.ChannelPoints ? twitchEvent?.Message[0]?.ChannelPointsName : null);
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
                StartCoroutine(PlayQueue());
            }
        }

        public void Dequeue(IAlert alert)
        {
            throw new System.NotImplementedException();
        }

        private IEnumerator PlayQueue()
        {
            IAlert alert = _queuedAlerts.Peek();
            _synchronizationContext.Send(SafeInvokeSpawn, alert);
            yield return new WaitForSecondsRealtime(alert.Lifeline);
            yield return new WaitForSecondsRealtime(alert.Flatline);
            _queuedAlerts.Dequeue();
            if (_queuedAlerts.Count != 0)
            {
                StartCoroutine(PlayQueue());
            }
        }

        void SafeInvokeSpawn(object alert) { (alert as IAlert)?.Spawn(); }
    }
}
