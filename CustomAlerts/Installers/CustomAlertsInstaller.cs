using CustomAlerts.Configuration;
using CustomAlerts.Queuing;
using CustomAlerts.Utilities;
using Zenject;

namespace CustomAlerts.Installers
{
    internal class CustomAlertsInstaller : Installer
    {
        private readonly PluginConfig _config;

        public CustomAlertsInstaller(PluginConfig config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ChatService>().AsSingle().NonLazy();
            Container.BindInstance(Plugin.ChatCoreMultiplexer).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<AlertObjectManager>().AsSingle().NonLazy();
            Container.Bind<IAlertQueue>().To<BasicQueueController>().FromNewComponentOnRoot().AsSingle().NonLazy();
        }
    }
}