using System;
using Zenject;
using UnityEngine;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using CustomAlerts.Configuration;

namespace CustomAlerts.Queuing
{
    public class BasicQueueController : MonoBehaviour, IAlertQueue
    {
        public float Delay => _config.AlertDelay;

        private Config _config;
        private event Action<IAlert> OnSpawn;
        private SynchronizationContext _synchronizationContext;
        private readonly Queue<IAlert> _queuedAlerts = new Queue<IAlert>();

        [Inject]
        public void Construct(Config config)
        {
            _config = config;
        }

        public void Awake()
        {
            _synchronizationContext = SynchronizationContext.Current;
            Plugin.Log.Info($"Created {nameof(BasicQueueController)}");
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
            // eh.
        }

        private IEnumerator PlayQueue()
        {
            IAlert alert = _queuedAlerts.Peek();
            _synchronizationContext.Send(SafeInvokeSpawn, alert);
            yield return new WaitForSecondsRealtime(alert.Lifeline + Delay);
            _synchronizationContext.Send(SafeInvokeDespawn, alert);
            _queuedAlerts.Dequeue();
            if (_queuedAlerts.Count != 0)
            {
                StartCoroutine(PlayQueue());
            }
        }

        void SafeInvokeSpawn(object alert) { (alert as CustomAlert).Spawn(); }
        void SafeInvokeDespawn(object alert) { (alert as CustomAlert).Destroy(); }
    }
}
