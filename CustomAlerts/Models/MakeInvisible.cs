using UnityEngine;
using System.Collections;

namespace CustomAlerts
{
    public class MakeInvisible : MonoBehaviour
    {
        private IEnumerator _coroutine;
        public void MakeInvisibleInSeconds(float seconds)
        {
            _coroutine = WaitAndDestroy(seconds);
            StartCoroutine(_coroutine);
        }

        IEnumerator WaitAndDestroy(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            print("WaitAndPrint " + Time.time);
            gameObject.SetActive(false);
            Destroy(this);
        }
    }
}