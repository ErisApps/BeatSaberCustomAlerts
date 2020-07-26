using CustomAlerts.Models;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace CustomAlerts.UI.Models
{
    public class ModelCell : CustomCellInfo
    {
        public CustomAlert Alert { get; }

        public ModelCell(CustomAlert alert) : base("", "", null)
        {
            Alert = alert;

            text = Alert.Descriptor.alertName;
            subtext = Alert.Descriptor.authorName + $" [{Alert.AlertType}]";
            icon = alert.Descriptor.icon;
        }
    }
}