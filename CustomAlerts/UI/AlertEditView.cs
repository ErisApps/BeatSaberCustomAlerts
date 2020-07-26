using Zenject;
using CustomAlerts.Models;
using CustomAlerts.Utilities;
using CustomAlerts.Configuration;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

namespace CustomAlerts.UI
{
    [HotReload]
    public class AlertEditView : BSMLAutomaticViewController
    {
        [UIParams]
        protected BSMLParserParams parserParams;

        [UIValue("delay?")]
        protected bool DelayOverridden { get; set; }

        [UIValue("delay")]
        protected float Delay { get; set; }

        private string _channelPointsName;
        [UIValue("channel-points-name")]
        protected string ChannelPointsName
        {
            get => _channelPointsName;
            set
            {
                _channelPointsName = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isChannel;
        [UIValue("is-channel")]
        protected bool IsChannel
        {
            get => _isChannel;
            set
            {
                _isChannel = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isDelayOverridden;
        [UIValue("override-delay")]
        protected bool IsDelayOverridden
        {
            get => _isDelayOverridden;
            set
            {
                _isDelayOverridden = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("delay-changed")]
        protected void DelayChanged(bool delayValue)
        {
            if (currentAlertValue != null)
            {
                IsDelayOverridden = currentAlertValue.OverrideDelay = delayValue;
            }
        }

        [UIAction("delay-time-changed")]
        protected void DelayTimeChanged(float delayValue)
        {
            if (currentAlertValue != null)
            {
                currentAlertValue.DelayOverrideTime = delayValue;
            }
        }

        protected AlertValue currentAlertValue;
        private AlertObjectManager _alertObjectManager;

        [Inject]
        protected void Construct(AlertObjectManager alertObjectManager)
        {
            _alertObjectManager = alertObjectManager;
        }

        public void SetAlert(CustomAlert alert)
        {
            AlertValue val = _alertObjectManager.ValueFromAlert(alert);

            currentAlertValue = val;
            if (alert.Descriptor.alertTriggerType == AlertType.ChannelPoints || !string.IsNullOrEmpty(alert.Descriptor.channelPointsName))
            {
                IsChannel = true;
                ChannelPointsName = "The channel point name is <color=#00ffff>" + alert.Descriptor.channelPointsName + "</color>. Name the reward on Twitch this in order to use it!";
            }
            else
            {
                IsChannel = false;
                ChannelPointsName = "";
            }
            Delay = currentAlertValue.DelayOverrideTime;
            DelayOverridden = IsDelayOverridden = currentAlertValue.OverrideDelay;
            parserParams.EmitEvent("update");
        }
    }
}