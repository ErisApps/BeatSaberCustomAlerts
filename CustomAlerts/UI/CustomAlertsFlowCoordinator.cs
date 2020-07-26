using HMUI;
using Zenject;
using UnityEngine;
using System.Collections;
using CustomAlerts.Models;
using CustomAlerts.Queuing;
using BeatSaberMarkupLanguage;

namespace CustomAlerts.UI
{
    public class CustomAlertsFlowCoordinator : FlowCoordinator
    {
        private InfoView _infoView;
        private IAlertQueue _alertQueue;
        private IEnumerator _hideScreens;
        private TweenPosition _screenTweener;
        private AlertListView _alertListView;
        private AlertEditView _alertEditView;
        private AlertDetailView _alertDetailView;
        private HierarchyManager _hierarchyManager;
        private ModalStateManager _modalStateManager;
        private NavigationController _navigationController;

        [Inject]
        protected void Construct(InfoView infoView, IAlertQueue alertQueue, AlertListView alertListView, AlertEditView alertEditView, AlertDetailView alertDetailView, HierarchyManager hierarchyManager, ModalStateManager modalStateManager, NavigationController navigationController)
        {
            _infoView = infoView;
            _alertQueue = alertQueue;
            _alertListView = alertListView;
            _alertEditView = alertEditView;
            _alertDetailView = alertDetailView;
            _hierarchyManager = hierarchyManager;
            _modalStateManager = modalStateManager;
            _navigationController = navigationController;
        }

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            if (firstActivation)
            {
                showBackButton = true;
                title = "Custom Alerts";
                ProvideInitialViewControllers(_navigationController, _infoView);
                PushViewControllerToNavigationController(_navigationController, _alertListView);
            }
            _alertListView.DidSelectAlert += AlertListView_SelectedAlert;
            _alertDetailView.PreviewPressed += AlertDetailView_RequestedPreview;
        }

        private void AlertDetailView_RequestedPreview(CustomAlert alert)
        {
            HideAllScreens(alert.Lifeline);
            _alertQueue.Enqueue(new CustomAlert(alert.GameObject, alert.Descriptor, null));
        }

        protected override void DidDeactivate(DeactivationType deactivationType)
        {
            if (!_modalStateManager.IsPresented)
            {
                base.DidDeactivate(deactivationType);
                _alertListView.DidSelectAlert -= AlertListView_SelectedAlert;
                _alertDetailView.PreviewPressed -= AlertDetailView_RequestedPreview;
                ForceUnhideScreen();
            }
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
        }

        private void AlertListView_SelectedAlert(CustomAlert alert)
        {
            if (!_alertDetailView.isInViewControllerHierarchy)
            {
                PushViewControllerToNavigationController(_navigationController, _alertDetailView);
            }
            _alertDetailView.SetAlert(alert);
            if (!_alertEditView.isInViewControllerHierarchy)
            {
                SetRightScreenViewController(_alertEditView);
            }
            _alertEditView.SetAlert(alert);
        }

        protected void HideAllScreens(float time = 1f)
        {
            if (_hideScreens != null)
            {
                StopCoroutine(_hideScreens);
                _hideScreens = null;
            }
            _hideScreens = HideScreens(time);
            StartCoroutine(_hideScreens);
        }

        protected IEnumerator HideScreens(float time)
        {
            _screenTweener = _hierarchyManager.gameObject.AddComponent<TweenPosition>();
            _screenTweener._duration = .75f;
            _screenTweener.TargetPos = new Vector3(0f, -5f, 0f);
            yield return new WaitForSecondsRealtime(time);
            _screenTweener.TargetPos = Vector3.zero;
            yield return new WaitForSecondsRealtime(0.75f);
            ForceUnhideScreen();
        }

        protected void ForceUnhideScreen()
        {
            if (_hideScreens != null)
            {
                StopCoroutine(_hideScreens);
                _hideScreens = null;
                if (_screenTweener != null)
                {
                    Destroy(_screenTweener);
                    _screenTweener = null;
                }
                _hierarchyManager.gameObject.transform.position = Vector3.zero;
            }
        } 
    }
}