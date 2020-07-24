using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using UnityEngine.SceneManagement;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;
using WebSocketSharp;
using BS_Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityStreamlabs;
using Streamlabs.Utilities;
using TMPro;
using BeatSaberMarkupLanguage.Settings;
using BS_Utils.Utilities;

namespace Streamlabs
{

    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin instance { get; private set; }
        internal static string Name => "Streamlabs";

        internal static BS_Utils.Utilities.Config config = new BS_Utils.Utilities.Config("Streamlabs");

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger)
        {
            instance = this;
            Logger.log = logger;
            Logger.log.Debug("Logger initialized.");
        }

        #region BSIPA Config
        //Uncomment to use BSIPA's config
        /*
        [Init]
        public void InitWithConfig(Config conf)
        {
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Logger.log.Debug("Config loaded");
        }
        */
        #endregion

        [OnStart]
        public void OnApplicationStart()
        {
            Logger.log.Debug("OnApplicationStart");
            // LoadAlerts();
            QueueController queueController = new GameObject("QueueController").AddComponent<QueueController>();
            QueueController.SafeSpawn += AlertObjectLoader.SafeSpawn;
            //BS_Utils.Utilities.BSEvents.gameSceneLoaded += OnGameSceneLoaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            // Init AssetBundles
            AlertObjectLoader.Load();
            // Init Streamlabs Webhook 
            UnityStreamlabs.Streamlabs.Connect();
            UnityStreamlabs.Streamlabs.OnEvent += AlertObjectLoader.SpawnAlertFromEvent;
            // Init Chat for channel points
            ChatService.StartChatService();
            ChatService.OnEvent += AlertObjectLoader.SpawnChannelPointsAlert;
            ChatService.OnNormalEvent += AlertObjectLoader.SpawnAlertFromEvent;

            BSMLSettings.instance.AddSettingsMenu("Streamlabs", "Streamlabs.UI.Settings.bsml", UI.Settings.instance);
            //CreateMenu();
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Logger.log.Debug("OnApplicationQuit");

        }
        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
            if (nextScene.name == "MenuViewControllers" && prevScene.name == "EmptyTransition")
            {
                Console.WriteLine("Adding Streamlabs Settings...");
                //BSMLSettings.instance.AddSettingsMenu("Streamlabs Alerts", "Streamlabs.UI.settings.bsml", UI.Settings.instance);
            }
        }
    }
}
