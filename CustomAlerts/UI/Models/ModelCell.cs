using CustomAlerts.Models;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace CustomAlerts.UI.Models
{
    public class ModelCell : CustomCellInfo
    {
        public CustomAlert Alert { get; }

        public ModelCell(CustomAlert alert)
            : base(alert.Descriptor.alertName, $"{alert.Descriptor.authorName} [{alert.AlertType}]", BeatSaberMarkupLanguage.Utilities.LoadSpriteFromTexture(alert.Descriptor.icon))
        {
            Alert = alert;
        }
    }
}