using Bolognese.Desktop.Contracts;
using Caliburn.Micro;

namespace Bolognese.Desktop.ViewModels
{
    public class TrayShellViewModel : Conductor<object>, IShell
    {
        readonly IWindowManager _manager;
        readonly IConductActiveItem _realShell;

        public TrayShellViewModel(IWindowManager manager, IConductActiveItem realShell)
        {
            _manager = manager;
            _realShell = realShell;
        }

        protected override void OnActivate()
        {
            ShowWindow();
        }

        public void ShowWindow()
        {
            _manager.ShowWindow(_realShell);
        }

        public void ExitApp()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
