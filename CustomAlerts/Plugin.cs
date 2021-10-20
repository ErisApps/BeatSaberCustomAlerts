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
using IPALogger = IPA.Logging.Logger;
using BeatSaberMarkupLanguage.MenuButtons;
using SiraUtil.Zenject;
using Config = CustomAlerts.Configuration.Config;

namespace CustomAlerts
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }
        internal static MenuButton MenuButton { get; set; }
        internal static ChatServiceMultiplexer ChatCoreMultiplexer { get; set; }

        [Init]
        public Plugin(Conf conf, IPALogger logger, Zenjector zenjector)
        {
            Log = logger;
            var chatcore = ChatCoreInstance.Create();
            ChatCoreMultiplexer = chatcore.RunAllServices();

            zenjector.OnApp<CustomAlertsInstaller>().WithParameters(conf.Generated<Config>());
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
                BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(flowCoordinator);
            }
            else
            {
                Log.Error("Unable to find flow coordinator! Cannot show Custom Alerts Flow Coordinator.");
            }
        }
    }
}