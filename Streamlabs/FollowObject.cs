using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Streamlabs
{
    class FollowObject : MonoBehaviour
    {
        public static FollowObject instance { get; private set; }
        private void Awake()
        {
            GameObject.DontDestroyOnLoad(this); // Don't destroy this object on scene changes
            instance = this;
            Destroy(this.gameObject, 10);

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.parent = this.transform;
            cube.transform.position = new Vector3(0, 2f, 0);
            cube.transform.localScale = new Vector3(2, 2, 2);
        }

    }
}
