using BeatSaberMarkupLanguage.Attributes;
using BS_Utils.Utilities;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Parser;
using System;
using Streamlabs;
using System.Collections.Generic;
using System.Linq;

namespace Streamlabs.UI
{
    class Settings : PersistentSingleton<Settings>
    {
        //helper functions for displaying the options
        public static List<object> GetAlerts(AlertDescriptor.AlertType alertType)
        {
            List<object> followAlerts = new List<object>();
            followAlerts.Add("Disabled");
            foreach (CustomAlert alert in Utilities.AlertObjectLoader.CustomAlerts)
            {
                if (alert.Descriptor.AlertTriggerType == alertType)
                {
                    followAlerts.Add(alert.Descriptor.AlertName);
                }
            }
            return followAlerts;
        }
        public void SaveAllAlerts()
        {
            string[] alertVars = {
                followValue,
                subValue,
                bitValue,
                raidValue,
                hostValue,
            };
            AlertDescriptor.AlertType[] alertTypes = {
                AlertDescriptor.AlertType.Follow,
                AlertDescriptor.AlertType.Subscription,
                AlertDescriptor.AlertType.Bits,
                AlertDescriptor.AlertType.Raids,
                AlertDescriptor.AlertType.Host
            };

            for (int i = 0; i < alertVars.Length; i++)
            {
                SaveAlert(alertVars[i], alertTypes[i]);
            }
        }
        public void SaveAlert(string input, AlertDescriptor.AlertType alertType)
        {
            Utilities.AlertObjectLoader.SetAlertByType(input, alertType);
        }
        public static string GetAlertName(AlertDescriptor.AlertType alertType)
        {
            CustomAlert alert = Utilities.AlertObjectLoader.GetAlertByType(alertType);
            if(alert != null)
            {
                return alert.Descriptor.AlertName;
            }
            else
            {
                return "Disabled";
            }
        }

        [UIValue("follow-options")]
        private List<object> followOptions = GetAlerts(AlertDescriptor.AlertType.Follow);

        [UIValue("follow-value")]
        private string followValue = GetAlertName(AlertDescriptor.AlertType.Follow);

        [UIValue("sub-options")]
        private List<object> subOptions = GetAlerts(AlertDescriptor.AlertType.Subscription);

        [UIValue("sub-value")]
        private string subValue = GetAlertName(AlertDescriptor.AlertType.Subscription);

        [UIValue("bit-options")]
        private List<object> bitOptions = GetAlerts(AlertDescriptor.AlertType.Bits);

        [UIValue("bit-value")]
        private string bitValue = GetAlertName(AlertDescriptor.AlertType.Bits);

        [UIValue("raid-options")]
        private List<object> raidOptions = GetAlerts(AlertDescriptor.AlertType.Raids);

        [UIValue("raid-value")]
        private string raidValue = GetAlertName(AlertDescriptor.AlertType.Raids);

        [UIValue("host-options")]
        private List<object> hostOptions = GetAlerts(AlertDescriptor.AlertType.Host);

        [UIValue("host-value")]
        private string hostValue = GetAlertName(AlertDescriptor.AlertType.Host);

        [UIAction("#apply")]
        public void OnApply() => SaveAllAlerts();
        public void Awake()
        {

        }
    }
}
