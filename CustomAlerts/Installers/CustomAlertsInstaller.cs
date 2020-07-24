using Zenject;
using ChatCore.Interfaces;
using CustomAlerts.Queuing;
using CustomAlerts.Utilities;
using CustomAlerts.Streamlabs;

namespace CustomAlerts.Installers
{
    public class CustomAlertsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInstance(Configuration.Config.Instance).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ChatService>().AsSingle();
            Container.BindInstance(Plugin.ChatCoreMultiplexer).AsSingle();
            Container.Bind<AlertObjectLoader>().AsSingle().NonLazy();
            Container.Bind<IWebSocketService>().To<WSSSocketProvider>().AsSingle();
            Container.Bind<IAlertQueue>().To<BasicQueueController>().FromNewComponentOnRoot().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<StreamlabsClient>().AsSingle().Lazy();
        }
    }
}