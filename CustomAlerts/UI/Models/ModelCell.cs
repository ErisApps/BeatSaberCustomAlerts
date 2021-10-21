using CustomAlerts.Models;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace CustomAlerts.UI.Models
{
    public class ModelCell : CustomCellInfo
    {
        public CustomAlert Alert { get; }

        public ModelCell(CustomAlert alert) : base(alert.Descriptor.alertName, $"{alert.Descriptor.authorName} [{alert.AlertType}]", null)
        {
            Alert = alert;

            // TODO: Fix icon got changed into Sprite instead of Texture2D
            /*text = Alert.Descriptor.alertName;
            subtext = $"{Alert.Descriptor.authorName} [{Alert.AlertType}]";
            icon = alert.Descriptor.icon;*/
        }
    }
}