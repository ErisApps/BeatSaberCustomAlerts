using Zenject;
using CustomAlerts.Queuing;
using CustomAlerts.Utilities;
using CustomAlerts.Streamlabs;
using UnityEngine;

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

    public class CustomAlertsInstallerNormal : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInstance(Configuration.Config.Instance).AsSingle().NonLazy();
            Container.BindInstance(Plugin.ChatCoreMultiplexer).AsSingle().NonLazy();
            Container.BindInstance(Plugin.AlertObjectManager).AsSingle().NonLazy();
            Container.BindInstance(Plugin.SocketProvider).AsSingle().NonLazy();
            Container.BindInstance(Plugin.StreamlabsClient).AsSingle().NonLazy();
            Container.BindInstance(Plugin.AlertQueue).AsSingle().NonLazy();

            /*Container.BindInstance(Configuration.Config.Instance).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ChatService>().AsSingle().NonLazy();
            Container.BindInstance(Plugin.ChatCoreMultiplexer).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<AlertObjectManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<WSSSocketProvider>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<StreamlabsClient>().AsSingle().NonLazy();
            
            Container.Bind<IAlertQueue>().To<BasicQueueController>().FromNewComponentOn(new GameObject("Basic Queue Controller")).AsSingle().NonLazy();
        */
        }
    }
}