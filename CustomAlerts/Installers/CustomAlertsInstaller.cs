using CatCore;
using CustomAlerts.Configuration;
using CustomAlerts.Queuing;
using CustomAlerts.Utilities;
using Zenject;

namespace CustomAlerts.Installers
{
    internal class CustomAlertsInstaller : Installer
    {
        private readonly PluginConfig _config;
        private readonly CatCoreInstance _catCoreInstance;

        public CustomAlertsInstaller(PluginConfig config, CatCoreInstance catCoreInstance)
        {
            _config = config;
            _catCoreInstance = catCoreInstance;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();
            Container.BindInstance(_catCoreInstance).AsSingle();
            Container.BindInterfacesAndSelfTo<ChatService>().AsSingle();
            Container.BindInterfacesAndSelfTo<AlertObjectManager>().AsSingle();
            Container.Bind<IAlertQueue>().To<BasicQueueController>().FromNewComponentOnRoot().AsSingle().NonLazy();
        }
    }
}