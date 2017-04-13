using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Bolognese.Desktop.Contracts;
using System.Configuration;
using Bolognese.Desktop.ViewModels;

namespace Bolognese.Desktop
{
    public class BologneseBoostrapper : BootstrapperBase
    {
        readonly SimpleContainer _container = new SimpleContainer();

        public BologneseBoostrapper()
        {
            Initialize();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void Configure()
        {
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<ITrackManager, PomodoroManager>();
            _container.Singleton<ISongFactory, SongFactory>();
            _container.PerRequest<IShell, ShellViewModel>();

            _container.Singleton<Screen, SmallPlayerViewModel>("player");
            _container.PerRequest<Screen, ConfigurationViewModel>("config");
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<IShell>();
        }
    }
}
