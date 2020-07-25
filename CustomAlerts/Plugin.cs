using IPA;
using Zenject;
using ChatCore;
using IPA.Config.Stores;
using ChatCore.Services;
using ChatCore.Interfaces;
using CustomAlerts.Queuing;
using CustomAlerts.Installers;
using Conf = IPA.Config.Config;
using CustomAlerts.Configuration;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;
using Installer = SiraUtil.Zenject.Installer;
using CustomAlerts.Utilities;
using CustomAlerts.Streamlabs;
using UnityEngine;

namespace CustomAlerts
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }
        internal static Plugin Instance { get; private set; }
        internal static ChatServiceMultiplexer ChatCoreMultiplexer { get; set; }
        internal static ChatService ChatService { get; set; }
        internal static AlertObjectManager AlertObjectManager { get; set; }
        internal static IWebSocketService SocketProvider { get; set; }
        internal static StreamlabsClient StreamlabsClient { get; set; }
        internal static IAlertQueue AlertQueue { get; set; }

        [Init]
        public Plugin(Conf conf, IPALogger logger)
        {
            Log = logger;
            Instance = this;
            var chatcore = ChatCoreInstance.Create();
            Config.Instance = conf.Generated<Config>();
            ChatCoreMultiplexer = chatcore.RunAllServices();
            Installer.RegisterAppInstaller<CustomAlertsInstaller>(OnSceneContextPostInstall);
        }

        [OnEnable]
        public void OnEnable()
        {
            SceneManager.sceneLoaded += SceneLoaded;
            //Installer.GetSceneContextAsync(OnSceneContextPostInstall, "PCInit");
            
            /*
            ChatService = new ChatService(Config.Instance, ChatCoreMultiplexer);
            AlertObjectManager = new AlertObjectManager(Config.Instance);
            SocketProvider = new WSSSocketProvider();
            StreamlabsClient = new StreamlabsClient(Config.Instance, SocketProvider);
            AlertQueue = new GameObject("CustomAlerts | BasicQueueController").AddComponent<BasicQueueController>();
            Object.DontDestroyOnLoad(AlertQueue as BasicQueueController);*/
        }

        private void SceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            if (scene.name == "PCInit")
            {
                //Installer.GetSceneContextAsync(OnSceneContextPostInstall, "PCInit");
            }
        }

        [OnDisable]
        public void OnDisable()
        {
            /*ChatService.Dispose();
            AlertObjectManager.Dispose();
            SocketProvider.Dispose();
            StreamlabsClient.Dispose();
            Object.Destroy(AlertQueue as BasicQueueController);
            */
            Installer.UnregisterAppInstaller<CustomAlertsInstaller>();
            SceneManager.sceneLoaded -= SceneLoaded;
        }

        private void OnSceneContextPostInstall(SceneContext context)
        {
            //context.Container.Install<CustomAlertsInstallerNormal>();
            //context.Container.Resolve<IAlertQueue>();
        }
    }
}