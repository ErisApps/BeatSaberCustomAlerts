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
            Container.BindInstance(Configuration.Config.Instance).AsSingle();
            Container.BindInterfacesAndSelfTo<ChatService>().AsSingle();
            Container.BindInstance(Plugin.ChatCoreMultiplexer).AsSingle();
            Container.BindInterfacesAndSelfTo<AlertObjectManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<WSSSocketProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<StreamlabsClient>().AsSingle();
            Container.Bind<IAlertQueue>().To<BasicQueueController>().FromNewComponentOnRoot().AsSingle();
        }
    }
}