using IPA;
using ChatCore;
using UnityEngine;
using System.Linq;
using CustomAlerts.UI;
using IPA.Config.Stores;
using ChatCore.Services;
using CustomAlerts.Installers;
using BeatSaberMarkupLanguage;
using Conf = IPA.Config.Config;
using CustomAlerts.Configuration;
using IPALogger = IPA.Logging.Logger;
using BeatSaberMarkupLanguage.MenuButtons;
using Installer = SiraUtil.Zenject.Installer;

namespace CustomAlerts
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }
        internal static Plugin Instance { get; private set; }
        internal static MenuButton MenuButton { get; set; }
        internal static ChatServiceMultiplexer ChatCoreMultiplexer { get; set; }

        [Init]
        public Plugin(Conf conf, IPALogger logger)
        {
            Log = logger;
            Instance = this;
            var chatcore = ChatCoreInstance.Create();
            Config.Instance = conf.Generated<Config>();
            ChatCoreMultiplexer = chatcore.RunAllServices();

            MenuButton = new MenuButton("Custom Alerts", ShowMainFlowCoordinator);
        }

        [OnEnable]
        public void OnEnable()
        {
            Installer.RegisterAppInstaller<CustomAlertsInstaller>();
            Installer.RegisterMenuInstaller<CustomAlertsMenuInstaller>();

            MenuButtons.instance.RegisterButton(MenuButton);
        }

        [OnDisable]
        public void OnDisable()
        {
            Installer.UnregisterAppInstaller<CustomAlertsInstaller>();
            Installer.UnregisterAppInstaller<CustomAlertsMenuInstaller>();

            MenuButtons.instance.UnregisterButton(MenuButton);
        }

        public void ShowMainFlowCoordinator()
        {
            CustomAlertsFlowCoordinator flowCoordinator = Resources.FindObjectsOfTypeAll<CustomAlertsFlowCoordinator>().FirstOrDefault();
            if (flowCoordinator != null)
            {
                BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(flowCoordinator);
            }
            else
            {
                Log.Error("Unable to find flow coordinator! Cannot show Custom Alerts Flow Coordinator.");
            }
        }
    }
}