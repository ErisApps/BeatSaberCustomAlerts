using HMUI;
using Zenject;
using UnityEngine;
using CustomAlerts.UI;
using BeatSaberMarkupLanguage;

namespace CustomAlerts.Installers
{
    public class CustomAlertsMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            InfoView infoView = BeatSaberUI.CreateViewController<InfoView>();
            AlertListView alertListView = BeatSaberUI.CreateViewController<AlertListView>();
            AlertEditView alertEditView = BeatSaberUI.CreateViewController<AlertEditView>();
            AlertDetailView alertDetailView = BeatSaberUI.CreateViewController<AlertDetailView>();
            NavigationController navigationController = BeatSaberUI.CreateViewController<NavigationController>();
            CustomAlertsFlowCoordinator customAlertsFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<CustomAlertsFlowCoordinator>();

            Container.Bind<ModalStateManager>().AsSingle();
            InstallController<InfoView>(infoView);
            InstallController<AlertListView>(alertListView);
            InstallController<AlertEditView>(alertEditView);
            InstallController<AlertDetailView>(alertDetailView);
            InstallController<NavigationController>(navigationController);
            InstallController<CustomAlertsFlowCoordinator>(customAlertsFlowCoordinator);
        }

        public void InstallController<T>(Component controller)
        {
            Container.BindInstance((T)(object)controller).AsSingle().NonLazy();
            Container.InjectGameObject(controller.gameObject);
        }
    }
}