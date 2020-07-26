using Zenject;
using CustomAlerts.Queuing;
using CustomAlerts.Utilities;
using CustomAlerts.Streamlabs;

namespace CustomAlerts.Installers
{
    public class CustomAlertsInstaller : MonoInstaller
    {
        public static bool FirstBindingInstalled { get; private set; } = false;

        public override void InstallBindings()
        {
            FirstBindingInstalled = true;
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