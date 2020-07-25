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
    public class BasicQueueController : MonoBehaviour, IAlertQueue
    {
        private StreamlabsClient _streamlabsClient;
        private AlertObjectManager _alertObjectLoader;
        private SynchronizationContext _synchronizationContext;
        private readonly Queue<IAlert> _queuedAlerts = new Queue<IAlert>();

        [Inject]
        public void Construct(StreamlabsClient streamlabsClient, AlertObjectManager alertObjectLoader)
        {
            _streamlabsClient = streamlabsClient;
            _alertObjectLoader = alertObjectLoader;
            _synchronizationContext = SynchronizationContext.Current;

            _streamlabsClient.OnEvent += OnEvent;

            Plugin.Log.Notice("Queue Controller Contructed");
        }

        public void OnDestry()
        {
            _streamlabsClient.OnEvent -= OnEvent;
        }

        private void OnEvent(StreamlabsEvent streamlabsEvent)
        {
            CustomAlert alert = _alertObjectLoader.GetAlertByType(streamlabsEvent.AlertType);
            AlertData alertData = _alertObjectLoader.Process(alert, streamlabsEvent);
            if (alertData.canSpawn)
            {
                CustomAlert newAlert = new CustomAlert(alert.GameObject, alert.Descriptor, streamlabsEvent)
                {
                    Flatline = alertData.delay
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

        void SafeInvokeSpawn(object alert) { (alert as IAlert).Spawn(); }
        void SafeInvokeDespawn(object alert) { (alert as IAlert).Destroy(); }
    }
}
