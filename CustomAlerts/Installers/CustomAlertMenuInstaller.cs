using HMUI;
using Zenject;
using CustomAlerts.UI;
using SiraUtil;

namespace CustomAlerts.Installers
{
	internal class CustomAlertsMenuInstaller : Installer
	{
		public override void InstallBindings()
		{
			Container.Bind<ModalStateManager>().AsSingle();
			Container.Bind<InfoView>().FromNewComponentAsViewController().AsSingle();
			Container.Bind<AlertListView>().FromNewComponentAsViewController().AsSingle();
			Container.Bind<AlertEditView>().FromNewComponentAsViewController().AsSingle();
			Container.Bind<AlertDetailView>().FromNewComponentAsViewController().AsSingle();
			Container.Bind<NavigationController>().FromNewComponentAsViewController().AsSingle();
			Container.BindInterfacesTo<CustomAlertsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
		}
	}
}