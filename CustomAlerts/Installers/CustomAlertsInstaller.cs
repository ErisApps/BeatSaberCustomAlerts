using Zenject;
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
            Container.BindInterfacesAndSelfTo<ChatService>().AsSingle().NonLazy();
            Container.BindInstance(Plugin.ChatCoreMultiplexer).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<AlertObjectManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<WSSSocketProvider>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<StreamlabsClient>().AsSingle().NonLazy();
            Container.Bind<IAlertQueue>().To<BasicQueueController>().FromNewComponentOnRoot().AsSingle().NonLazy();
        }
    }
}