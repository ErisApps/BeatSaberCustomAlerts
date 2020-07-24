using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

namespace Streamlabs
{
    public class ReplaceTextWithUsername : MonoBehaviour
    {
        public string textToReplace = "{username}";
        public void Replace(string username)
        {
            //Console.WriteLine("NICNEIOCNEOICA");

            //TextMeshPro textMesh = this.gameObject.AddComponent<TextMeshPro>();
            /*if (textMesh)
            {
                textMesh.text = textMesh.text.Replace(textToReplace, username);
            }*/
        }
    }
}
