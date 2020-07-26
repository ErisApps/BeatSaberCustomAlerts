﻿using System;
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
                string assetPath = Path.Combine(UnityGame.UserDataPath, "CustomAlerts");
                Directory.CreateDirectory(assetPath);

                IEnumerable<string> alertFilter = new List<string> { "*.alert" };
                CustomAlertFiles = GetFileNames(assetPath, alertFilter, SearchOption.AllDirectories, true);
                Plugin.Log.Notice($"{CustomAlertFiles.Count()} alert(s) found.");

                Alerts = LoadCustomAlerts(CustomAlertFiles);

                foreach (var alert in Alerts)
                {
                    if (!_config.Alerts.Any(a => a.AlertType == alert.AlertType && a.Value == (string.IsNullOrEmpty(alert.Descriptor.channelPointsName) ? alert.Descriptor.alertName : alert.Descriptor.channelPointsName)))
                    {
                        _config.Alerts.Add(new AlertValue
                        {
                            Enabled = false,
                            OverrideDelay = false,
                            AlertType = alert.AlertType,
                            DelayOverrideTime = _config.AlertDelay,
                            Value = string.IsNullOrEmpty(alert.Descriptor.channelPointsName) ? alert.Descriptor.alertName : alert.Descriptor.channelPointsName
                        });
                    }
                }
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

        public AlertValue ValueFromAlert(CustomAlert alert)
        {
            AlertValue value = _config.Alerts.FirstOrDefault(a => a.AlertType == alert.AlertType && a.Value == (string.IsNullOrEmpty(alert.Descriptor.channelPointsName) ? alert.Descriptor.alertName : alert.Descriptor.channelPointsName));
            if (value == null)
            {
                value = new AlertValue
                {
                    Enabled = false,
                    OverrideDelay = false,
                    AlertType = alert.AlertType,
                    DelayOverrideTime = _config.AlertDelay,
                    Value = string.IsNullOrEmpty(alert.Descriptor.channelPointsName) ? alert.Descriptor.alertName : alert.Descriptor.channelPointsName
                };
                _config.Alerts.Add(value);
            }
            return value;
        }

        public void ResetAlertValues(AlertType type)
        {
            _config.Alerts.Where(a => a.AlertType == type).ToList().ForEach(a => a.Enabled = false);
        }

        public int ActiveAlertCount(AlertType type)
        {
            return _config.Alerts.Count(a => a.AlertType == type && a.Enabled == true);
        }

        public AlertData Process(CustomAlert alert, StreamlabsEvent streamEvent)
        {
            AlertData data = new AlertData
            {
                canSpawn = false
            };
            if (alert == null || alert.Descriptor == null || alert.GameObject == null)
            {
                return data;
            }
            AlertValue value = _config.Alerts.FirstOrDefault(a => a.Enabled && a.AlertType == alert.AlertType && (a.Value == alert.Descriptor.alertName || a.Value == alert.Descriptor.channelPointsName));
            if (value != null)
            {
                data.canSpawn = true;
                if (value.AlertType == AlertType.ChannelPoints && streamEvent != null)
                {
                    data.canSpawn = alert.Descriptor.channelPointsName.ToLower().Trim() == streamEvent.Message[0].ChannelPointsName.ToLower().Trim();
                }
                data.delay = value.OverrideDelay ? value.DelayOverrideTime : _config.AlertDelay;
            }
            return data;
        }

        public CustomAlert GetAlertByType(AlertType type, string valueSpecific = null)
        {
            AlertValue value;
            if (!string.IsNullOrEmpty(valueSpecific))
            {
                value = _config.Alerts.FirstOrDefault(a => a.Enabled == true && a.AlertType == type && a.Value == valueSpecific);
            }
            else
            {
                value = _config.Alerts.FirstOrDefault(a => a.Enabled == true && a.AlertType == type);
            }
            if (value != null)
            {
                var alert = Alerts.FirstOrDefault(ca => ca.Descriptor != null && (ca.Descriptor.alertName == value.Value || ca.Descriptor.channelPointsName == value.Value));
                return alert;
            }
            return null;
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