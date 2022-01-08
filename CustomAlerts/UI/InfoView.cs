using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using Hive.Versioning;
using IPA.Loader;
using SiraUtil.Zenject;
using UnityEngine;
using Zenject;

namespace CustomAlerts.UI
{
    [HotReload]
    public class InfoView : BSMLAutomaticViewController
    {
        private Version _version;

        [Inject]
        protected void Construct(UBinder<Plugin, PluginMetadata> pluginMetadata)
        {
            _version = pluginMetadata.Value.HVersion;
        }
        
        [UIAction("join-discord")]
        protected void AlertClicked()
        {
            Application.OpenURL("https://discord.gg/MwqzVb");
        }

        [UIValue("mod-version")]
        internal string Version => $"Version {_version}";
    }
}