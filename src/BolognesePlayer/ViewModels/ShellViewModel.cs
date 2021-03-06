﻿using Caliburn.Micro;
using Bolognese.Desktop.Contracts;
using Bolognese.Common.Configuration;

namespace Bolognese.Desktop
{
    public class ShellViewModel : Conductor<object>.Collection.OneActive, IShell, IHandle<ShowPlayerRequested>
    {
        readonly IEventAggregator _eventBus;
        IConfigurationSettings _settings;

        public void ShowLargePlayer()
        {
            // show the player interface
        }

        public void CloseWindow()
        {
            TryClose();
        }

        public void ShowConfiguration()
        {
            // show the configuration interface
            var configScreen = IoC.Get<Screen>("config");
            ActivateItem(configScreen);
        }

        public void ShowSmallPlayer()
        {
            var player = IoC.Get<Screen>("player");
            ActivateItem(player);
        }

        void IHandle<ShowPlayerRequested>.Handle(ShowPlayerRequested message)
        {
            ShowSmallPlayer();
        }

        public ShellViewModel(IEventAggregator events, IConfigurationSettings settings)
        {
            _settings = settings;
            _eventBus = events;
            _eventBus.Subscribe(this);

            if (_settings.AudioFilePath != string.Empty)
            {
                ShowSmallPlayer();
            }
            else
            {
                ShowConfiguration();
            }
        }
    }
}
