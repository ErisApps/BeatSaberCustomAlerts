using HMUI;
using System;
using Zenject;
using UnityEngine;
using System.Linq;
using CustomAlerts.Models;
using CustomAlerts.UI.Models;
using CustomAlerts.Utilities;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;

namespace CustomAlerts.UI
{
    [HotReload]
    public class AlertListView : BSMLAutomaticViewController
    {
        public event Action<CustomAlert> DidSelectAlert;

        private AlertObjectManager _alertObjectManager;

        [UIComponent("alert-list")]
        protected CustomListTableData alertList;

        [UIAction("alert-clicked")]
        protected void AlertClicked(TableView _, int row)
        {
            DidSelectAlert?.Invoke((alertList.data[row] as ModelCell).Alert);
        }

        [Inject]
        protected void Construct(AlertObjectManager alertObjectManager)
        {
            _alertObjectManager = alertObjectManager;
        }

        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            base.DidActivate(firstActivation, type);
            if (firstActivation)
            {
                rectTransform.anchorMin = new Vector3(0.5f, 0, 0);
                rectTransform.anchorMax = new Vector3(0.5f, 1, 0);
                rectTransform.sizeDelta = new Vector3(70, 0, 0);
            }
            alertList.data.Clear();
            alertList.data.AddRange(_alertObjectManager.Alerts.Select(ca => new ModelCell(ca)).ToList().OrderBy(a => a.Alert.AlertType));
            alertList.tableView.ReloadData();
        }
    }
}