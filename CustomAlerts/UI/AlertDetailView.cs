using System;
using Zenject;
using UnityEngine;
using UnityEngine.UI;
using CustomAlerts.Models;
using CustomAlerts.Utilities;
using CustomAlerts.Configuration;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

namespace CustomAlerts.UI
{
    [HotReload]
    internal class AlertDetailView : BSMLAutomaticViewController
    {
        [UIParams]
        protected BSMLParserParams parserParams;

        [UIComponent("alert-image")]
        protected Image alertImage;

        private string _alertTitle = "Alert Title";
        [UIValue("alert-title")]
        protected string AlertTitle
        {
            get => _alertTitle;
            set
            {
                _alertTitle = value;
                NotifyPropertyChanged();
            }
        }

        private string _alertAuthor = "Alert Author";
        [UIValue("alert-author")]
        protected string AlertAuthor
        {
            get => _alertAuthor;
            set
            {
                _alertAuthor = value;
                NotifyPropertyChanged();
            }
        }

        private string _alertDescription = "Alert Description";
        [UIValue("alert-description")]
        protected string AlertDescription
        {
            get => _alertDescription;
            set
            {
                _alertDescription = value;
                NotifyPropertyChanged();
            }
        }

        private string _toggleText = "Disabled";
        [UIValue("toggle-text")]
        protected string ToggleText
        {
            get => _toggleText;
            set
            {
                _toggleText = value;
                NotifyPropertyChanged();
            }
        }

        private string _toggleColor = "red";
        [UIValue("toggle-color")]
        protected string ToggleColor
        {
            get => _toggleColor;
            set
            {
                _toggleColor = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("toggle-enable")]
        protected void ToggleEnable()
        {
            if (_alertObjectManager.ActiveAlertCount(_customAlert.AlertType) > 0 && !_alertObjectManager.ValueFromAlert(_customAlert).Enabled && _customAlert.AlertType != AlertType.ChannelPoints)
            {
                parserParams.EmitEvent("show-warning");
                _modalStateManager.IsPresented = true;
                ModalText = $"This will set <color=#00ffff>{_customAlert.Descriptor.alertName}</color>\nas the default for the type <color=#00ffff>{_customAlert.AlertType}</color>.\nAre you sure?";
            }
            else
            {
                SetEnable();
            }
        }

        private string _modalText = "This will set an alert as the default for the type. Are you sure?";
        [UIValue("modal-text")]
        protected string ModalText
        {
            get => _modalText;
            set
            {
                _modalText = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("set-enable")]
        protected void SetEnable()
        {
            if (_customAlert != null)
            {
                AlertValue val = _alertObjectManager.ValueFromAlert(_customAlert);
                if (_customAlert.AlertType != AlertType.ChannelPoints && _modalStateManager.IsPresented)
                {
                    Cancel();
                    _alertObjectManager.ResetAlertValues(_customAlert.AlertType);
                }
                if (!val.Enabled)
                {
                    ToggleText = "Enabled";
                    ToggleColor = "green";
                }
                else
                {
                    ToggleText = "Disabled";
                    ToggleColor = "red";
                }
                val.Enabled = !val.Enabled;
            }
        }

        [UIAction("close-warning")]
        protected void Cancel()
        {
            parserParams.EmitEvent("hide-warning");
            _modalStateManager.IsPresented = false;
        }

        [UIAction("preview")]
        protected void Preview()
        {
            PreviewPressed?.Invoke(_customAlert);
        }

        public event Action<CustomAlert> PreviewPressed;

        private CustomAlert _customAlert;
        private ModalStateManager _modalStateManager;
        private AlertObjectManager _alertObjectManager;
        
        [Inject]
        protected void Construct(ModalStateManager modalStateManager, AlertObjectManager alertObjectManager)
        {
            _modalStateManager = modalStateManager;
            _alertObjectManager = alertObjectManager;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (firstActivation)
            {
                rectTransform.anchorMin = new Vector3(0.5f, 0, 0);
                rectTransform.anchorMax = new Vector3(0.5f, 1, 0);
                rectTransform.sizeDelta = new Vector3(70, 0, 0);
            }
        }

        public void SetAlert(CustomAlert alert)
        {
            if (_modalStateManager.IsPresented)
            {
                parserParams.EmitEvent("hide-warning");
                _modalStateManager.IsPresented = false;
            }

            _customAlert = alert;
            alertImage.sprite = alert.Sprite;
            AlertTitle = alert.Descriptor.alertName ?? "No Title";
            AlertAuthor = string.IsNullOrEmpty(alert.Descriptor.authorName) ? "No Author" : "by " + alert.Descriptor.authorName;
            AlertDescription = alert.Descriptor.description ?? "No Description";

            AlertValue val = _alertObjectManager.ValueFromAlert(alert);
            if (val.Enabled)
            {
                ToggleText = "Enabled";
                ToggleColor = "green";
            }
            else
            {
                ToggleText = "Disabled";
                ToggleColor = "red";
            }
        }
    }
}