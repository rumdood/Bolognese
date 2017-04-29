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

        private const string ShortBreakText = "Short Break";
        private const string LongBreakText = "Long Break";

        private readonly IEventAggregator _events;
        private readonly IConfigurationSettings _settings;
        private Playlist _currentPlaylist;
        private double _currentSegmentProgress = 0;
        private string _currentSongTitle = "";
        private PomodoroSegment _currentSegment;
        private Brush _progressBrush = Brushes.Green;
        private PackIconKind _playPauseFront = PlayIcon;
        private PackIconKind _playPauseBack = PauseIcon;
        private bool _isRunning = false;
        private TimeSpan _timeRemaining = TimeSpan.FromSeconds(0);
        private int _pomodoroCount = 0;

        public int PomodoroCount
        {
            get
            {
                return _pomodoroCount;
            }
            set
            {
                _pomodoroCount = value;
                NotifyOfPropertyChange(() => PomodoroCount);
            }
        }

        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                _isRunning = value;
                NotifyOfPropertyChange(() => IsRunning);
                NotifyOfPropertyChange(() => CanRestart);

                var foo = CanRestart;
            }
        }

        public bool CanRestart
        {
            get
            {
                bool playing = !IsRunning;
                bool canPlay = CanPlayPause;
                bool someProgress = CurrentSegmentProgress < 100 && CurrentSegmentProgress > 0;

                return playing && canPlay && someProgress;
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
            get
            {
                string title = string.Empty;

                if (CurrentSegment != null)
                {
                    switch (CurrentSegment.SegmentType)
                    {
                        case PomodoroSegmentType.Working:
                            title = _currentSongTitle;
                            break;
                        case PomodoroSegmentType.ShortBreak:
                            title = ShortBreakText;
                            break;
                        case PomodoroSegmentType.LongBreak:
                            title = LongBreakText;
                            break;
                        default:
                            title = string.Empty;
                            break;
                    }
                }

                return title;
            }
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
                NotifyOfPropertyChange(() => CanRestart);
            }
        }

        public bool CanPlayPause
        {
            get
            {
                if (_currentSegment != null 
                    && (_currentSegment.SegmentType == PomodoroSegmentType.Working
                    || (_currentSegment.SegmentType != PomodoroSegmentType.Working 
                        && _currentSegment.Status == SegmentStatus.ReadyToStart)))
                {
                    return true;
                }

                return false;
            }
        }

        public PomodoroSegment CurrentSegment
        {
            get { return _currentSegment; }
            private set
            {
                if (_currentSegment != value)
                {
                    if (_currentSegment != null && _currentSegment.SegmentType == PomodoroSegmentType.Working)
                    {
                        PomodoroCount += 1;
                    }

                    _currentSegment = value;
                    NotifyOfPropertyChange(() => CurrentSongTitle);
                    NotifyOfPropertyChange(() => CurrentSegment);
                    NotifyOfPropertyChange(() => CanPlayPause);
                    SetRunningStatus();

                    PlayPauseFrontIcon = PlayIcon;

                    switch (CurrentSegment.SegmentType)
                    {
                        case PomodoroSegmentType.ShortBreak:
                        case PomodoroSegmentType.LongBreak:
                            PlayPauseBackIcon = ShortBreakIcon;

                            break;
                        case PomodoroSegmentType.Working:
                            PlayPauseBackIcon = PauseIcon;
                            break;
                        default:
                            PlayPauseFrontIcon = ErrorIcon;
                            break;
                    }
                }
            }
        }

        private void SetRunningStatus()
        {
            if (CurrentSegment.Status == SegmentStatus.Running)
            {
                IsRunning = true;

                if (CurrentSegment.SegmentType != PomodoroSegmentType.Working)
                {
                    PlayPauseFrontIcon = ShortBreakIcon;
                }
            }
            else
            {
                IsRunning = false;
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

            if (request.Action == SegmentRequestAction.StartNext 
                || CurrentSegment.SegmentType == PomodoroSegmentType.Working)
            {
                _events.PublishOnUIThread(request);
            }
        }

        public void RestartSegment()
        {
            if (CurrentSegment.SegmentType == PomodoroSegmentType.Working)
            {
                SegmentRequest request = new SegmentRequest()
                {
                    Action = SegmentRequestAction.Restart
                };

                _events.PublishOnUIThread(request);
            }
        }

        public void NextWorkingSegment()
        {
            if (CurrentSegment.SegmentType == PomodoroSegmentType.Working)
            {
                SegmentRequest request = new SegmentRequest()
                {
                    Action = SegmentRequestAction.StartNextWorkSegment
                };

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
            SetRunningStatus();
        }
    }
}
