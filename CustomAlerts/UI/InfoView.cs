using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using UnityEngine;

namespace CustomAlerts.UI
{
    [HotReload]
    public class InfoView : BSMLAutomaticViewController
    {
        [UIAction("join-discord")]
        protected void AlertClicked()
        {
            Application.OpenURL("https://discord.gg/MwqzVb");
        }
    }
}