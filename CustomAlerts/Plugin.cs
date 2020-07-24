using IPA;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using CustomAlerts.Installers;
using Conf = IPA.Config.Config;
using CustomAlerts.Configuration;
using IPALogger = IPA.Logging.Logger;
using ChatCore.Interfaces;
using ChatCore;
using ChatCore.Services.Twitch;
using IPA.Utilities;
using ChatCore.Services;

namespace CustomAlerts
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }
        internal static Plugin Instance { get; private set; }
        internal static IWebSocketService WebSocketService { get; set; }
        internal static ChatServiceMultiplexer ChatCoreMultiplexer { get; set; }

        [Init]
        public Plugin(Conf conf, IPALogger logger)
        {
            Log = logger;
            Instance = this;
            Config.Instance = conf.Generated<Config>();

            var chatcore = ChatCoreInstance.Create();
            ChatCoreMultiplexer = chatcore.RunAllServices();
        }

        [OnEnable]
        public void OnEnable()
        {
            Installer.RegisterMenuInstaller<CustomAlertsInstaller>();
        }

        [OnDisable]
        public void OnDisable()
        {
            Installer.UnregisterMenuInstaller<CustomAlertsInstaller>();
        }

    }
}
