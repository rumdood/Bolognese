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

                    if (CurrentStatus == PlayingStatus.ShortBreak)
                    {
                        PlayPauseFrontIcon = ShortBreakIcon;
                    }
                    else if (CurrentStatus == PlayingStatus.LongBreak)
                    {
                        PlayPauseFrontIcon = LongBreakIcon;
                    }
                    else if (CurrentStatus == PlayingStatus.Error)
                    {
                        PlayPauseFrontIcon = ErrorIcon;
                    }
                    else
                    {
                        PlayPauseFrontIcon = PlayIcon;
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

            if (_settings.AudioFilePath != string.Empty 
                || Directory.Exists(_settings.AudioFilePath))
            {
                Playlist playlist = new Playlist();
                playlist.Name = "New";
                DirectoryInfo directory = new DirectoryInfo(_settings.AudioFilePath);
                foreach (FileInfo file in directory.GetFiles("*.mp3"))
                {
                    Song song = new Song(file.FullName, file.Name);
                    playlist.Songs.Add(song);
                }

                CurrentPlaylist = playlist;
                _manager.OpenPlaylist(playlist);
            }
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
                case PlayingStatus.Stopped:
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
        }

        private double CalculateSegmentProgress(TimeSpan totalTime, TimeSpan progress)
        {
            double segmentProgress = (progress.TotalSeconds / totalTime.TotalSeconds) * 100;

            if (CurrentStatus == PlayingStatus.ShortBreak || CurrentStatus == PlayingStatus.LongBreak)
            {
                segmentProgress = 100 - segmentProgress; // reverse this so that it counts down instead of up
            }

            return segmentProgress;
        }
    }
}
