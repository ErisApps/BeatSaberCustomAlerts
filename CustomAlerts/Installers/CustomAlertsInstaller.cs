using CustomAlerts.Configuration;
using Zenject;
using CustomAlerts.Queuing;
using CustomAlerts.Utilities;
using CustomAlerts.Streamlabs;

namespace CustomAlerts.Installers
{
    internal class CustomAlertsInstaller : Installer
    {
        private readonly Config _config;

        public CustomAlertsInstaller(Config config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ChatService>().AsSingle().NonLazy();
            Container.BindInstance(Plugin.ChatCoreMultiplexer).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<AlertObjectManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<WSSSocketProvider>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<StreamlabsClient>().AsSingle().NonLazy();
            Container.Bind<IAlertQueue>().To<BasicQueueController>().FromNewComponentOnRoot().AsSingle().NonLazy();
        }
    }
}