using ChatCore;
using ChatCore.Services;
using CustomAlerts.Configuration;
using CustomAlerts.Installers;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Logging;
using SiraUtil.Zenject;

namespace CustomAlerts
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static Logger Log { get; private set; }
        internal static ChatServiceMultiplexer ChatCoreMultiplexer { get; set; }

        [Init]
        public Plugin(Config config, Logger logger, Zenjector zenjector)
        {
            Log = logger;
            var chatcore = ChatCoreInstance.Create();
            ChatCoreMultiplexer = chatcore.RunAllServices();

            zenjector.OnApp<CustomAlertsInstaller>().WithParameters(config.Generated<PluginConfig>());
            zenjector.OnMenu<CustomAlertsMenuInstaller>();
        }

        [OnEnable, OnDisable]
        public void OnStateChanged()
        {
            // Nop, Zenject is pretty pog imho
        }
    }
}