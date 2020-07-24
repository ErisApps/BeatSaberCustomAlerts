using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Threading;

namespace Streamlabs
{
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.
    /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
    /// </summary>
    public class QueueController : MonoBehaviour
    {
        public static QueueController instance { get; private set; }

        private static Queue<CustomAlert> queuedAlerts = new Queue<CustomAlert>();

        public static event Action<CustomAlert> SafeSpawn;

        private static SynchronizationContext unitySyncContext;
        private void Awake()
        {
            if (instance != null)
            {
                Logger.log?.Warn($"Instance of {this.GetType().Name} already exists, destroying.");
                GameObject.DestroyImmediate(this);
                return;
            }
            unitySyncContext = SynchronizationContext.Current;
            GameObject.DontDestroyOnLoad(this); // Don't destroy this object on scene changes
            instance = this;
            Logger.log?.Debug($"{name}: Awake()");
        }
        
        public void QueueAlert(CustomAlert alert)
        {
            queuedAlerts.Enqueue(alert);
            if(queuedAlerts.Count == 1)
            {
                StartCoroutine(PlayQueue());
            }
        }
        void SafeInvokeSpawn(object alert) => SafeSpawn?.Invoke(alert as CustomAlert);

        IEnumerator PlayQueue()
        {
            CustomAlert alert = queuedAlerts.Peek();
            Console.WriteLine(unitySyncContext.ToString());
            //alert.Spawn();
            unitySyncContext.Send(SafeInvokeSpawn, alert);
            yield return new WaitForSeconds(alert.Descriptor.AlertLifetime+2);
            queuedAlerts.Dequeue();
            if(queuedAlerts.Count != 0)
            {
                StartCoroutine(PlayQueue());
            }
        }

    }
}
