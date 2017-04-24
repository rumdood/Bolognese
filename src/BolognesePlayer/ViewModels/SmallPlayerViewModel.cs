using System.Windows.Media;
using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using System;
using Bolognese.Common.Media;
using Bolognese.Common.Configuration;
using Bolognese.Common.Pomodoro;

namespace Bolognese.Desktop.ViewModels
{
    public class SmallPlayerViewModel : Screen, IHandle<MediaStatusChanged>, IHandle<SegmentProgressChanged>, IHandle<SegmentStatusChanged>
    {
        private const PackIconKind PlayIcon = PackIconKind.Play;
        private const PackIconKind PauseIcon = PackIconKind.Pause;
        private const PackIconKind ShortBreakIcon = PackIconKind.Alarm;
        private const PackIconKind LongBreakIcon = PackIconKind.Alarm;
        private const PackIconKind ErrorIcon = PackIconKind.Alert;

        private readonly IEventAggregator _events;
        private readonly IConfigurationSettings _settings;
        private Playlist _currentPlaylist;
        private double _currentSegmentProgress = 0;
        private string _currentSongTitle = "";
        private PomodoroSegment _currentSegment;
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

        public PomodoroSegment CurrentSegment
        {
            get { return _currentSegment; }
            private set
            {
                if (_currentSegment != value)
                {
                    _currentSegment = value;
                    NotifyOfPropertyChange(() => CurrentSegment);
                    SetPlayingStatus();

                    switch (CurrentSegment.SegmentType)
                    {
                        case PomodoroSegmentType.ShortBreak:
                        case PomodoroSegmentType.LongBreak:
                            PlayPauseFrontIcon = ShortBreakIcon;
                            break;
                        case PomodoroSegmentType.Working:
                            PlayPauseFrontIcon = PlayIcon;
                            break;
                        default:
                            PlayPauseFrontIcon = ErrorIcon;
                            break;
                    }
                }
            }
        }

        private void SetPlayingStatus()
        {
            if (CurrentSegment.SegmentType == PomodoroSegmentType.Working)
            {
                if (CurrentSegment.Status == SegmentStatus.Running)
                {
                    IsPlaying = true;
                }
                else
                {
                    IsPlaying = false;
                }
            }
            else
            {
                IsPlaying = false;
            }
        }

        public SmallPlayerViewModel(IEventAggregator events, IPomodoroManager manager, IConfigurationSettings settings)
        {
            _events = events;
            _events.Subscribe(this);

            _settings = settings;
            manager.Initialize();
        }

        public void PlayPause()
        {
            if (CurrentSegment.SegmentType == PomodoroSegmentType.Working)
            {
                SegmentRequest request = new SegmentRequest();
                switch (CurrentSegment.Status)
                {
                    case SegmentStatus.ReadyToStart:
                        request.Action = SegmentRequestAction.StartNext;
                        break;
                    case SegmentStatus.Stopped:
                        request.Action = SegmentRequestAction.Resume;
                        break;
                    case SegmentStatus.Running:
                        request.Action = SegmentRequestAction.Pause;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                _events.PublishOnUIThread(request);
            }
        }

        public void Handle(MediaStatusChanged playerStatus)
        {
            if (playerStatus.CurrentSong != null)
            {
                CurrentSongTitle = playerStatus.CurrentSong.Title;
            }
        }

        public void Handle(SegmentProgressChanged segmentProgress)
        {
            CurrentSegment = segmentProgress.Segment;
            CurrentSegmentProgress = CalculateSegmentProgress(segmentProgress.Segment.Duration, segmentProgress.Segment.Progress);
            var remaining = segmentProgress.Segment.Duration.Subtract(segmentProgress.Segment.Progress);
            TimeRemaining = remaining;
        }

        private double CalculateSegmentProgress(TimeSpan totalTime, TimeSpan progress)
        {
            double segmentProgress = (progress.TotalSeconds / totalTime.TotalSeconds) * 100;

            if (CurrentSegment.SegmentType == PomodoroSegmentType.Working)
            {
                segmentProgress = 100 - segmentProgress; // reverse this so that it counts down instead of up
            }

            return segmentProgress;
        }

        public void Handle(SegmentStatusChanged message)
        {
            CurrentSegment.Status = message.Status;
            SetPlayingStatus();
        }
    }
}
