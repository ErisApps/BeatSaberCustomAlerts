using CatCore;
using CustomAlerts.Configuration;
using CustomAlerts.Installers;
using Hive.Versioning;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using IPA.Logging;
using SiraUtil.Zenject;

namespace CustomAlerts
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static Logger Log { get; private set; }

        [Init]
        public Plugin(Logger logger, Config config, PluginMetadata pluginMetadata, Zenjector zenjector)
        {
            Log = logger;

            zenjector.UseLogger(logger);
            
            zenjector.Install<CustomAlertsInstaller>(Location.App, config.Generated<PluginConfig>(), new UBinder<Plugin, Version>(pluginMetadata.HVersion), CatCoreInstance.Create());
            zenjector.Install<CustomAlertsMenuInstaller>(Location.Menu);
        }

        [OnEnable, OnDisable]
        public void OnStateChanged()
        {
            // Nop, Zenject is pretty pog imho
        }
    }
}