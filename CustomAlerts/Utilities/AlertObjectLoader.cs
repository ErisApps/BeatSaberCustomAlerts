using System;
using System.IO;
using System.Linq;
using IPA.Utilities;
using CustomAlerts.Queuing;
using CustomAlerts.Streamlabs;
using CustomAlerts.Configuration;
using System.Collections.Generic;

namespace CustomAlerts.Utilities
{
    public class AlertObjectLoader
    {
        private readonly Config _config;
        private readonly IAlertQueue _alertQueue;
        private readonly StreamlabsClient _streamlabsClient;
        public bool Loaded { get; private set; } = false;
        public IList<CustomAlert> Alerts { get; private set; }
        public IEnumerable<string> CustomAlertFiles { get; private set; }

        public AlertObjectLoader(Config config, IAlertQueue alertQueue, StreamlabsClient streamlabsClient)
        {
            _config = config;
            _alertQueue = alertQueue;
            Alerts = new List<CustomAlert>();
            _streamlabsClient = streamlabsClient;
            CustomAlertFiles = Enumerable.Empty<string>();

            Load();
        }

        public void Load()
        {
            if (!Loaded)
            {
                _streamlabsClient.OnEvent += SpawnAlertFromEvent;

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
                _streamlabsClient.OnEvent -= SpawnAlertFromEvent;
                Alerts.Clear();
                CustomAlertFiles = Enumerable.Empty<string>();
                Loaded = false;
            }
        }

        public void SetAlertByType(string input, AlertType type)
        {
            AlertValue value = _config.Alerts.FirstOrDefault(a => a.AlertType == type);
            if (value != null)
            {
                value.Value = input;
            }
        }

        public CustomAlert GetAlertByType(AlertType type)
        {
            AlertValue value = _config.Alerts.FirstOrDefault(a => a.Value != "Disabled" && a.AlertType == type);
            if (value != null)
            {
                return Alerts.FirstOrDefault(ca => ca.Descriptor.alertName == value.Value);
            }
            return null;
        }

        public void SpawnAlertFromEvent(StreamlabsEvent streamlabsEvent)
        {
            

            AlertType type = streamlabsEvent.AlertType;
            var alertValue = _config.Alerts.FirstOrDefault(a => a.Value != "Disabled" && a.AlertType == type);
            if (alertValue != null)
            {
                Plugin.Log.Info("alert value not null");
                var alert = Alerts.FirstOrDefault(ca => ca.Descriptor.alertName == alertValue.Value);
                if (alert != null)
                {
                    Plugin.Log.Info("alert not null");
                    CustomAlert newAlert = new CustomAlert(alert.GameObject, alert.Descriptor, streamlabsEvent);
                    _alertQueue.Enqueue(newAlert);            
}
            }
        }

        public void SpawnChannelPointsAlert(StreamlabsEvent streamlabsEvent)
        {
            AlertType type = streamlabsEvent.AlertType;
            var alert = Alerts.FirstOrDefault(ca => ca.Descriptor.channelPointsName.ToLower().Trim() == streamlabsEvent.Message[0].ChannelPointsName.ToLower().Trim());
            if (alert != null)
            {
                alert.StreamEvent = streamlabsEvent;
                // at least for now, i'm not gonna queue channel points. I think it'll be funnier when they're not queued.
                alert.Spawn();
            }
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
                        Value = "Disabled"
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
    }
}