using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Streamlabs
{
    public class AlertDescriptor : MonoBehaviour
    {
        public enum AlertType
        {
            Follow,
            Subscription,
            Donation,
            Host,
            Bits,
            Raids,
            ChannelPoints,
            Other
        };
        public AlertType AlertTriggerType;
        public float AlertLifetime = 10;
        public string AlertName = "Alert";
        public string AuthorName = "Author";
        public string Description = string.Empty;
        public Texture2D Icon;
        public string ChannelPointsName = string.Empty;
    }
}
