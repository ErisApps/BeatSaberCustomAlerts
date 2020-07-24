using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using IPA.Utilities;
using TMPro;
using UnityStreamlabs;
using System.Collections;

namespace Streamlabs.Utilities
{
    public class AlertObjectLoader
    {
        public static bool IsLoaded { get; private set; }
        /*public static int SelectedFollowAlert { get; internal set; } = 0;
        public static int SelectedSubAlert { get; internal set; } = 0;
        public static int SelectedBitAlert { get; internal set; } = 0;
        public static int SelectedRaidAlert { get; internal set; } = 0;*/

        public static IList<CustomAlert> CustomAlerts { get; private set; }
        public static IEnumerable<string> CustomNoteFiles { get; private set; } = Enumerable.Empty<string>();

        //private static Queue<CustomAlert> queuedAlerts = new Queue<CustomAlert>();

        /// <summary>
        /// Load all CustomNotes
        /// </summary>
        internal static void Load()
        {
            if (!IsLoaded)
            {
                LoadAlerts();
                string AssetPath = Path.Combine(UnityGame.InstallPath, "CustomAlerts");
                Directory.CreateDirectory(AssetPath);

                IEnumerable<string> noteFilter = new List<string> { "*.alert" };
                CustomNoteFiles = GetFileNames(AssetPath, noteFilter, SearchOption.AllDirectories, true);
                Logger.log.Debug($"{CustomNoteFiles.Count()} external note(s) found.");

                IList<CustomAlert> TempCustomAlerts = LoadCustomNotes(CustomNoteFiles);
                CustomAlerts = new List<CustomAlert>();
                for (int i = 0; i < TempCustomAlerts.Count; i++)
                {
                    CustomAlert alert = TempCustomAlerts[i];
                    // UnityEngine.Object.DontDestroyOnLoad(alert.gameObject);
                    if(alert.gameObject != null)
                    {
                        Console.WriteLine("Loaded " + alert.Descriptor.AlertName);
                        CustomAlerts.Add(alert);
                    }
                    //TextHelper.TakeCareOfIt(alert);
                }
                Logger.log.Debug($"{CustomAlerts.Count} total note(s) loaded.");
                // load config
                //int loadedFollow = Plugin.config.GetInt("Streamlabs", "SelectedFollowAlert", SelectedFollowAlert);
                //SelectedFollowAlert = loadedFollow;
                /*if (Configuration.CurrentlySelectedAlert != null)
                {
                    int numberOfNotes = CustomNoteObjects.Count;
                    for (int i = 0; i < numberOfNotes; i++)
                    {
                        if (CustomNoteObjects[i].FileName == Configuration.CurrentlySelectedAlert)
                        {
                            SelectedAlert = i;
                            break;
                        }
                    }
                }*/
                //SelectedAlert = 0;

                IsLoaded = true;
            }
        }

        private class AlertValue
        {
            public AlertDescriptor.AlertType alertType;
            public string value;

        }
        private static List<AlertValue> alerts = new List<AlertValue>();
        public static void SetAlertByType(string inputValue, AlertDescriptor.AlertType alertType)
        {
            foreach (AlertValue alertValue in alerts)
            {
                if (alertValue.alertType == alertType)
                {
                    alertValue.value = inputValue;
                    Plugin.config.SetString("StreamlabsAlerts", alertType.ToString() + "Value", inputValue);
                }
            }
        }
        public static CustomAlert GetAlertByType(AlertDescriptor.AlertType alertType)
        {
            foreach (AlertValue alertValue in alerts)
            {
                if (alertValue.value != "Disabled")
                {
                    if (alertValue.alertType == alertType)
                    {
                        // found the proper one. get by name.
                        foreach (CustomAlert alert in AlertObjectLoader.CustomAlerts)
                        {
                            if (alert.Descriptor.AlertName == alertValue.value)
                            {
                                Console.WriteLine(alert.Descriptor.AlertLifetime);
                                return alert;
                            }
                        }
                    }
                }
            }
            return null;
        }
        public static void SpawnAlertFromEvent(StreamlabsEvent streamlabsEvent)
        {
            AlertDescriptor.AlertType alertType = streamlabsEvent.AlertType;
            foreach (AlertValue alertValue in alerts)
            {
                if (alertValue.value != "Disabled")
                {
                    if (alertValue.alertType == alertType)
                    {
                        // found the proper one. get by name.
                        foreach (CustomAlert alert in AlertObjectLoader.CustomAlerts)
                        {
                            if (alert.Descriptor.AlertName == alertValue.value)
                            {
                                // alert.streamEvent = streamlabsEvent;
                                // duplicate the alert.
                                CustomAlert newAlert = new CustomAlert(alert.gameObject, alert.Descriptor, streamlabsEvent);
                                QueueController.instance.QueueAlert(newAlert);
                                //alert.Spawn();
                            }
                        }
                    }
                }
            }
        }

        public static void SpawnChannelPointsAlert(StreamlabsEvent streamlabsEvent)
        {
            AlertDescriptor.AlertType alertType = streamlabsEvent.AlertType;
            foreach (CustomAlert alert in AlertObjectLoader.CustomAlerts)
            {
                if (alert.Descriptor.ChannelPointsName.ToLower().Trim() == streamlabsEvent.message[0].channelPointsName.ToLower().Trim())
                {
                    alert.streamEvent = streamlabsEvent;
                    //QueueController.instance.QueueAlert(alert);
                    // at least for now, i'm not gonna queue channel points. I think it'll be funnier when they're not queued.
                     alert.Spawn();
                }
            }
        }

        public static void SafeSpawn(CustomAlert alert)
        {
            if (alert != null)
            {
                if(alert.gameObject != null)
                {
                    alert.Spawn();
                }
            }
        }
        public static void LoadAlerts()
        {
            foreach (AlertDescriptor.AlertType alertType in (AlertDescriptor.AlertType[])Enum.GetValues(typeof(AlertDescriptor.AlertType)))
            {
                AlertValue alertValue = new AlertValue();
                alertValue.alertType = alertType;
                alertValue.value = Plugin.config.GetString("StreamlabsAlerts", alertType.ToString() + "Value", "Disabled");
                alerts.Add(alertValue);
                //int savedAlertValue = Plugin.config.GetInt("Streamlabs", "SelectedFollowAlert", SelectedFollowAlert);
            }
        }
        /*public static CustomAlert AlertFromType(StreamlabsEvent streamlabsEvent)
        {
            int alertIndex = -1;
            switch (streamlabsEvent.type)
            {
                case "channelpoints":
                    for(int i = 0; i < CustomAlerts.Count; i++)
                    {
                        CustomAlert alert = CustomAlerts[i];
                        if (alert.Descriptor.ChannelPointsName.ToLower().Trim() == streamlabsEvent.message[0].channelPointsName.ToLower().Trim())
                        {
                            alertIndex = i;
                        }
                    }
                    break;
                case "follow":
                    alertIndex = SelectedFollowAlert;
                    break;
                case "subscription":
                    alertIndex = SelectedSubAlert;
                    break;
                case "bits":
                    alertIndex = SelectedBitAlert;
                    break;
                case "raid":
                    alertIndex = SelectedRaidAlert;
                    break;
            }
            if (alertIndex != -1)
            {
                CustomAlert whichAlert = AlertObjectLoader.CustomAlerts[alertIndex];
                if (whichAlert != null)
                {
                    whichAlert.streamEvent = streamlabsEvent;
                    return whichAlert;
                }
            }
            return null;
        }*/
        public static IEnumerable<string> GetFileNames(string path, IEnumerable<string> filters, SearchOption searchOption, bool returnShortPath = false)
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
        /// <summary>
        /// Reload all CustomNotes
        /// </summary>
        /*internal static void Reload()
        {
            Logger.log.Debug("Reloading the NoteAssetLoader");

            Clear();
            Load();
        }

        /// <summary>
        /// Clear all loaded CustomNotes
        /// </summary>
        internal static void Clear()
        {
            int numberOfObjects = CustomNoteObjects.Count;
            for (int i = 0; i < numberOfObjects; i++)
            {
                CustomNoteObjects[i].Destroy();
                CustomNoteObjects[i] = null;
            }

            IsLoaded = false;
            SelectedAlert = 0;
            CustomNoteObjects = new List<CustomNote>();
            CustomNoteFiles = Enumerable.Empty<string>();
        }*/

        private static IList<CustomAlert> LoadCustomNotes(IEnumerable<string> customNoteFiles)
        {
            IList<CustomAlert> customNotes = new List<CustomAlert>
            {
                //new CustomNote("DefaultNotes"),
            };

            foreach (string customNoteFile in customNoteFiles)
            {
                try
                {
                    CustomAlert newNote = new CustomAlert(customNoteFile);
                    if (newNote != null)
                    {
                        customNotes.Add(newNote);
                    }
                }
                catch (Exception ex)
                {
                    Logger.log.Warn($"Failed to Load Custom Note with name '{customNoteFile}'.");
                    Logger.log.Warn(ex);
                }
            }

            return customNotes;
        }
    }
}
