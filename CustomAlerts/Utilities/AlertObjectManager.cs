using System;
using System.IO;
using System.Linq;
using IPA.Utilities;
using CustomAlerts.Models;
using CustomAlerts.Streamlabs;
using CustomAlerts.Configuration;
using System.Collections.Generic;

namespace CustomAlerts.Utilities
{
    public class AlertObjectManager : IDisposable
    {
        private readonly Config _config;
        public bool Loaded { get; private set; } = false;
        public IList<CustomAlert> Alerts { get; private set; }
        public IEnumerable<string> CustomAlertFiles { get; private set; }

        public AlertObjectManager(Config config)
        {
            _config = config;
            Alerts = new List<CustomAlert>();
            CustomAlertFiles = Enumerable.Empty<string>();

            Load();
        }

        public void Load()
        {
            if (!Loaded)
            {
                Plugin.Log.Notice("Object Manager Loading...");
                FillDefaultAlerts();
                string assetPath = Path.Combine(UnityGame.UserDataPath, "CustomAlerts");
                Directory.CreateDirectory(assetPath);

                IEnumerable<string> alertFilter = new List<string> { "*.alert" };
                CustomAlertFiles = GetFileNames(assetPath, alertFilter, SearchOption.AllDirectories, true);
                Plugin.Log.Notice($"{CustomAlertFiles.Count()} alert(s) found.");

                Alerts = LoadCustomAlerts(CustomAlertFiles);
            }
            Loaded = true;
        }

        public void Unload()
        {
            if (Loaded)
            {
                Plugin.Log.Notice("Object Manager Unloading...");
                Alerts.ToList().ForEach(a => { a.Destroy(); Plugin.Log.Info("Destroyed: " + a.FileName); });
                Alerts.Clear();
                CustomAlertFiles = Enumerable.Empty<string>();
                Loaded = false;
            }
        }

        public AlertData Process(CustomAlert alert, StreamlabsEvent streamEvent)
        {
            AlertData data = new AlertData
            {
                canSpawn = false
            };
            if (alert == null || alert.Descriptor == null || alert.GameObject == null || streamEvent == null)
            {
                return data;
            }
            AlertValue value = _config.Alerts.FirstOrDefault(a => a.Enabled && a.AlertType == streamEvent.AlertType && a.Value == alert.Descriptor.alertName);
            if (value != null)
            {
                data.canSpawn = true;
                if (value.AlertType == AlertType.ChannelPoints)
                {
                    data.canSpawn = alert.Descriptor.channelPointsName.ToLower().Trim() == streamEvent.Message[0].ChannelPointsName.ToLower().Trim();
                }
                data.delay = value.OverrideDelay ? value.DelayOverrideTime : _config.AlertDelay;
            }
            return data;
        }

        /*public void SetAlertByType(string input, AlertType type)
        {
            AlertValue value = _config.Alerts.FirstOrDefault(a => a.AlertType == type);
            if (value != null)
            {
                value.Value = input;
            }
        }*/

        public CustomAlert GetAlertByType(AlertType type)
        {
            AlertValue value = _config.Alerts.FirstOrDefault(a => a.Enabled == true && a.AlertType == type);
            if (value != null)
            {
                var alert = Alerts.FirstOrDefault(ca => ca.Descriptor != null && ca.Descriptor.alertName == value.Value);
                return alert;
            }
            return null;
        }

        private void FillDefaultAlerts()
        {
            foreach (AlertType alertType in (AlertType[])Enum.GetValues(typeof(AlertType)))
            {
                if (!_config.Alerts.Any(a => a.AlertType == alertType))
                {
                    AlertValue alertValue = new AlertValue
                    {
                        AlertType = alertType,
                        Enabled = false
                    };
                    _config.Alerts.Add(alertValue);
                }
            }
        }

        private static IEnumerable<string> GetFileNames(string path, IEnumerable<string> filters, SearchOption searchOption, bool returnShortPath = false)
        {
            IList<string> filePaths = new List<string>();
            foreach (string filter in filters)
            {
                IEnumerable<string> directoryFiles = Directory.GetFiles(path, filter, searchOption);
                if (returnShortPath)
                {
                    foreach (string directoryFile in directoryFiles)
                    {
                        string filePath = directoryFile.Replace(path, "");
                        if (filePath.Length > 0 && filePath.StartsWith(@"\"))
                        {
                            filePath = filePath.Substring(1, filePath.Length - 1);
                        }
                        if (!string.IsNullOrWhiteSpace(filePath) && !filePaths.Contains(filePath))
                        {
                            filePaths.Add(filePath);
                        }
                    }
                }
                else
                {
                    filePaths = filePaths.Union(directoryFiles).ToList();
                }
            }
            return filePaths.Distinct();
        }

        private IList<CustomAlert> LoadCustomAlerts(IEnumerable<string> customAlertFiles)
        {
            IList<CustomAlert> customAlerts = new List<CustomAlert>();
            foreach (string caf in customAlertFiles)
            {
                try
                {
                    CustomAlert newAlert = new CustomAlert(caf);
                    if (newAlert != null && newAlert.GameObject != null)
                    {
                        customAlerts.Add(newAlert);
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.Warn($"Failed to load Custom Alert with the name {caf}. {e.Message}");
                }
            }
            return customAlerts;
        }

        public void Dispose()
        {
            Unload(); //woopsBiggasm
            Plugin.Log.Info("Unloading Complete");
        }
    }
}