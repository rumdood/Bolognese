using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using Bolognese.Desktop.Contracts;
using Bolognese.Desktop.ViewModels;
using Bolognese.Common.Media;
using Bolognese.Common.Pomodoro;
using Bolognese.Common.Configuration;
using Bolognese.Desktop.Configuration;

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

            _container.Handler<IConfigurationSettings>(_container => ConfigurationHelper.GetConfiguration());

            _container.Singleton<IPomodoroManager, PomodoroManager>();
            _container.Singleton<IPomodoroSegmentFactory, PomodoroSegmentFactory>();
            _container.Singleton<IMediaManager, TrackManager>();
            _container.Singleton<ISongFactory, SongFactory>();
            _container.PerRequest<IShell, ShellViewModel>();

            _container.Singleton<Alerter>();
            _container.Singleton<Screen, SmallPlayerViewModel>("player");
            _container.PerRequest<Screen, ConfigurationViewModel>("config");
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            _container.GetInstance<IPomodoroManager>();
            _container.GetInstance<IMediaManager>();
            _container.GetInstance<Alerter>();
            DisplayRootViewFor<IShell>();
        }
    }
}
