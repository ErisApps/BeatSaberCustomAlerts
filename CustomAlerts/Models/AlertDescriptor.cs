using UnityEngine;

namespace CustomAlerts
{
    public class AlertDescriptor : MonoBehaviour
    {
        public AlertType alertTriggerType;
        public float alertLifetime = 10f;
        public string alertName = "Alert";
        public string authorName = "Author";
        public string description = string.Empty;
        public string channelPointsName = string.Empty;
        public Texture2D icon;
    }
}