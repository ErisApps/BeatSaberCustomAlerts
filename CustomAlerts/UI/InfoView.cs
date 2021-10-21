using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using Hive.Versioning;
using SiraUtil.Zenject;
using UnityEngine;
using Zenject;

namespace CustomAlerts.UI
{
    [HotReload]
    public class InfoView : BSMLAutomaticViewController
    {
        private UBinder<Plugin, Version> _version;

        [Inject]
        protected void Construct(UBinder<Plugin, Version> version)
        {
            _version = version;
        }
        
        [UIAction("join-discord")]
        protected void AlertClicked()
        {
            Application.OpenURL("https://discord.gg/MwqzVb");
        }

        [UIValue("mod-version")]
        internal string Version => $"Version {_version.Value}";
    }
}