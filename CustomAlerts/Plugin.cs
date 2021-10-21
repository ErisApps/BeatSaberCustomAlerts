using System.Linq;
using BeatSaberMarkupLanguage.MenuButtons;
using ChatCore;
using ChatCore.Services;
using CustomAlerts.Configuration;
using CustomAlerts.Installers;
using CustomAlerts.UI;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using UnityEngine;
using Logger = IPA.Logging.Logger;

namespace CustomAlerts
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static Logger Log { get; private set; }
        internal static MenuButton MenuButton { get; set; }
        internal static ChatServiceMultiplexer ChatCoreMultiplexer { get; set; }

        [Init]
        public Plugin(Config config, Logger logger, Zenjector zenjector)
        {
            Log = logger;
            var chatcore = ChatCoreInstance.Create();
            ChatCoreMultiplexer = chatcore.RunAllServices();

            zenjector.OnApp<CustomAlertsInstaller>().WithParameters(config.Generated<PluginConfig>());
            zenjector.OnMenu<CustomAlertsMenuInstaller>();

            MenuButton = new MenuButton("Custom Alerts", ShowMainFlowCoordinator);
        }

        [OnEnable]
        public void OnEnable()
        {
            MenuButtons.instance.RegisterButton(MenuButton);
        }

        [OnDisable]
        public void OnDisable()
        {
            MenuButtons.instance.UnregisterButton(MenuButton);
        }

        public void ShowMainFlowCoordinator()
        {
            CustomAlertsFlowCoordinator flowCoordinator = Resources.FindObjectsOfTypeAll<CustomAlertsFlowCoordinator>().FirstOrDefault();
            if (flowCoordinator != null)
            {
                BeatSaberMarkupLanguage.BeatSaberUI.PresentFlowCoordinator(BeatSaberMarkupLanguage.BeatSaberUI.MainFlowCoordinator, flowCoordinator);
            }
            else
            {
                Log.Error("Unable to find flow coordinator! Cannot show Custom Alerts Flow Coordinator.");
            }
        }
    }
}