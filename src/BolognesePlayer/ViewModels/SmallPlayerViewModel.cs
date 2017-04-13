using System.IO;
using System.Windows.Media;
using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using Bolognese.Desktop.Tracks;
using System;

namespace Bolognese.Desktop.ViewModels
{
    public class SmallPlayerViewModel : Screen, IHandle<PlayerStatusChanged>, IHandle<SegmentProgressChanged>
    {
        private const PackIconKind PlayIcon = PackIconKind.Play;
        private const PackIconKind PauseIcon = PackIconKind.Pause;
        private const PackIconKind ShortBreakIcon = PackIconKind.Alarm;
        private const PackIconKind LongBreakIcon = PackIconKind.Alarm;
        private const PackIconKind ErrorIcon = PackIconKind.Alert;

        private readonly IEventAggregator _events;
        private readonly IConfigurationSettings _settings;
        private readonly ITrackManager _manager;
        private Playlist _currentPlaylist;
        private double _currentSegmentProgress = 0;
        private string _currentSongTitle = "";
        private PlayingStatus _currentStatus = PlayingStatus.Stopped;
        private Brush _progressBrush = Brushes.Green;
        private PackIconKind _playPauseFront = PlayIcon;
        private PackIconKind _playPauseBack = PauseIcon;
        private bool _isPlaying = false;
        private TimeSpan _timeRemaining = TimeSpan.FromSeconds(0);

        public bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
            set
            {
                _isPlaying = value;
                NotifyOfPropertyChange(() => IsPlaying);
            }
        }

        public TimeSpan TimeRemaining
        {
            get
            {
                return _timeRemaining;
            }
            set
            {
                _timeRemaining = value;
                NotifyOfPropertyChange(() => TimeRemaining);
            }
        }

        public PackIconKind PlayPauseFrontIcon
        {
            get
            {
                return _playPauseFront;
            }
            set
            {
                _playPauseFront = value;
                NotifyOfPropertyChange(() => PlayPauseFrontIcon);
            }
        }

        public PackIconKind PlayPauseBackIcon
        {
            get
            {
                return _playPauseBack;
            }
            set
            {
                _playPauseBack = value;
                NotifyOfPropertyChange(() => PlayPauseBackIcon);
            }
        }

        public Playlist CurrentPlaylist
        {
            get { return _currentPlaylist; }
            set
            {
                _currentPlaylist = value;
                NotifyOfPropertyChange(() => CurrentPlaylist);
            }
        }

        public string CurrentSongTitle
        {
            get { return _currentSongTitle; }
            set
            {
                _currentSongTitle = value;
                NotifyOfPropertyChange(() => CurrentSongTitle);
            }
        }

        public double CurrentSegmentProgress
        {
            get { return _currentSegmentProgress; }
            set
            {
                _currentSegmentProgress = value;
                NotifyOfPropertyChange(() => CurrentSegmentProgress);
            }
        }

        public PlayingStatus CurrentStatus
        {
            get { return _currentStatus; }
            private set
            {
                if (_currentStatus != value)
                {
                    _currentStatus = value;
                    NotifyOfPropertyChange(() => CurrentStatus);

                    if (CurrentStatus == PlayingStatus.Playing)
                    {
                        IsPlaying = true;
                    }
                    else
                    {
                        IsPlaying = false;
                    }

                    switch (CurrentStatus)
                    {
                        case PlayingStatus.ShortBreak:
                        case PlayingStatus.LongBreak:
                            PlayPauseFrontIcon = ShortBreakIcon;
                            break;
                        case PlayingStatus.ReadyToPlay:
                        case PlayingStatus.Paused:
                            PlayPauseFrontIcon = PlayIcon;
                            break;
                        default:
                            PlayPauseFrontIcon = ErrorIcon;
                            break;
                    }
                }
            }
        }

        public SmallPlayerViewModel(IEventAggregator events, ITrackManager manager)
        {
            _events = events;
            _events.Subscribe(this);

            _settings = BologneseConfigurationSettings.GetConfigurationSettings();
            _manager = manager;

            _manager.BuildPlaylist();
        }

        public void PlayPause()
        {
            switch (CurrentStatus)
            {
                case PlayingStatus.Playing:
                    _manager.Pause();
                    break;
                case PlayingStatus.Paused:
                    _manager.PlayCurrentTrack();
                    break;
                case PlayingStatus.ReadyToPlay:
                    _manager.PlayNextTrack();
                    break;
                default:
                    break;
            }
        }

        public void Handle(PlayerStatusChanged playerStatus)
        {
            CurrentStatus = playerStatus.CurrentStatus;
            CurrentSongTitle = playerStatus.CurrentTrackTitle;
        }

        public void Handle(SegmentProgressChanged segmentProgress)
        {
            CurrentSegmentProgress = CalculateSegmentProgress(segmentProgress.TotalTime, segmentProgress.Progress);
            TimeRemaining = segmentProgress.TotalTime.Subtract(segmentProgress.Progress);
        }

        private double CalculateSegmentProgress(TimeSpan totalTime, TimeSpan progress)
        {
            double segmentProgress = (progress.TotalSeconds / totalTime.TotalSeconds) * 100;

            if (CurrentStatus == PlayingStatus.Playing)
            {
                segmentProgress = 100 - segmentProgress; // reverse this so that it counts down instead of up
            }

            return segmentProgress;
        }
    }
}
