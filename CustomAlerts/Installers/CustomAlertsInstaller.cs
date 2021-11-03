using CatCore;
using CustomAlerts.Configuration;
using CustomAlerts.Queuing;
using CustomAlerts.Utilities;
using Hive.Versioning;
using IPA.Logging;
using SiraUtil;
using SiraUtil.Zenject;
using Zenject;

namespace CustomAlerts.Installers
{
    internal class CustomAlertsInstaller : Installer
    {
        private readonly Logger _logger;
        private readonly PluginConfig _config;
        private readonly UBinder<Plugin, Version> _version;
        private readonly CatCoreInstance _catCoreInstance;

        public CustomAlertsInstaller(Logger logger, PluginConfig config, UBinder<Plugin, Version> version, CatCoreInstance catCoreInstance)
        {
            _logger = logger;
            _config = config;
            _version = version;
            _catCoreInstance = catCoreInstance;
        }

        public override void InstallBindings()
        {
            Container.BindLoggerAsSiraLogger(_logger);
            Container.BindInstance(_config).AsSingle();
            Container.BindInstance(_version).AsSingle();
            Container.BindInstance(_catCoreInstance).AsSingle();
            Container.BindInterfacesAndSelfTo<ChatService>().AsSingle();
            Container.BindInterfacesAndSelfTo<AlertObjectManager>().AsSingle();
            Container.Bind<IAlertQueue>().To<BasicQueueController>().FromNewComponentOnRoot().AsSingle().NonLazy();
        }
    }
}