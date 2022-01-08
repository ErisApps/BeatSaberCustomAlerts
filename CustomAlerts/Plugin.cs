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
    [Plugin(RuntimeOptions.DynamicInit), NoEnableDisable]
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
    }
}