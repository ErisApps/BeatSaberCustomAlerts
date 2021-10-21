using CatCore;
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

        [Init]
        public Plugin(Config config, Logger logger, Zenjector zenjector)
        {
            Log = logger;

            zenjector.OnApp<CustomAlertsInstaller>().WithParameters(logger, config.Generated<PluginConfig>(), ChatCoreInstance.CreateInstance());
            zenjector.OnMenu<CustomAlertsMenuInstaller>();
        }

        [OnEnable, OnDisable]
        public void OnStateChanged()
        {
            // Nop, Zenject is pretty pog imho
        }
    }
}