using Bolognese.Common.Configuration;
using Caliburn.Micro;

namespace Bolognese.Desktop.ViewModels
{
    public class ConfigurationViewModel : Screen
    {
        IEventAggregator _eventBus;
        IConfigurationSettings _settings;

        public string AudioFilePath
        {
            get { return _settings.AudioFilePath; }
            set
            {
                _settings.AudioFilePath = value;
                NotifyOfPropertyChange(() => AudioFilePath);
            }
        }

        public bool Shuffle
        {
            get { return _settings.Shuffle; }
            set
            {
                _settings.Shuffle = value;
                NotifyOfPropertyChange(() => Shuffle);
            }
        }

        public int LongBreakCount
        {
            get { return _settings.LongBreakCount; }
            set
            {
                _settings.LongBreakCount = value;
                NotifyOfPropertyChange(() => LongBreakCount);
            }
        }

        public int LongBreakDuration
        {
            get { return _settings.LongBreakDuration; }
            set
            {
                _settings.LongBreakDuration = value;
                NotifyOfPropertyChange(() => LongBreakDuration);
            }
        }

        public int ShortBreakDuration
        {
            get { return _settings.ShortBreakDuration; }
            set
            {
                _settings.ShortBreakDuration = value;
                NotifyOfPropertyChange(() => ShortBreakDuration);
            }
        }

        public ConfigurationViewModel(IEventAggregator events, IConfigurationSettings settings)
        {
            _settings = settings;
            _eventBus = events;
        }

        public void SaveConfiguration()
        {
            _settings.Save();
            _eventBus.PublishOnUIThread(new ShowPlayerRequested());
        }

        public void CancelChanges()
        {
            _eventBus.PublishOnUIThread(new ShowPlayerRequested());
        }
    }
}
