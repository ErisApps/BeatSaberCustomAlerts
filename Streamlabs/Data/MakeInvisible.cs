using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Streamlabs
{

    class MakeInvisible : MonoBehaviour
    {
        private IEnumerator coroutine;
        public void makeInvisibleInSeconds(float seconds)
        {
            coroutine = WaitAndDestroy(seconds);
            StartCoroutine(coroutine);
        }

        IEnumerator WaitAndDestroy(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            print("WaitAndPrint " + Time.time);
            this.gameObject.SetActive(false);
            Destroy(this);
        }

    }
}
