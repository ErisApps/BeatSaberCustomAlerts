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
        public Plugin(Logger logger, Config config, Zenjector zenjector)
        {
            Log = logger;

            zenjector.UseLogger(logger);
            zenjector.UseMetadataBinder<Plugin>();

            zenjector.Install<CustomAlertsInstaller>(Location.App, config.Generated<PluginConfig>(), CatCoreInstance.Create());
            zenjector.Install<CustomAlertsMenuInstaller>(Location.Menu);
        }

        [OnEnable, OnDisable]
        public void OnStateChanged()
        {
            // Nop, Zenject is pretty pog imho
        }
    }
}