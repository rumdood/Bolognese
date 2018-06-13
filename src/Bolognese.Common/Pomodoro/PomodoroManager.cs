using Bolognese.Common.Configuration;
using Bolognese.Common.Media;
using Caliburn.Micro;
using System;
using System.Diagnostics;
using System.Timers;

namespace Bolognese.Common.Pomodoro
{
    public class PomodoroManager : IPomodoroManager, 
                                   IHandle<MediaStatusChanged>, 
                                   IHandle<SegmentRequest>, 
                                   IHandle<MediaProgress>
    {
        readonly IConfigurationSettings _settings;
        readonly IPomodoroSegmentFactory _factory;
        readonly IEventAggregator _events;

        Timer _segmentTimer;
        PomodoroSegment _currentSegment;
        Song _currentSong = null;

        public PomodoroManager(IConfigurationSettings settings, 
                               IPomodoroSegmentFactory factory, 
                               IEventAggregator events)
        {
            _settings = settings;
            _factory = factory;
            _events = events;
            _events.Subscribe(this);
            _segmentTimer = new Timer(200);
            _segmentTimer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_currentSegment.SegmentType != PomodoroSegmentType.Working)
            {
                _currentSegment.Progress = _currentSegment.Progress.Add(TimeSpan.FromMilliseconds(_segmentTimer.Interval));
            }

            BroadcastSegmentProgress();

            if (_currentSegment.Progress >= _currentSegment.Duration)
            {
                CompleteSegment();
            }
        }

        private void CompleteSegment()
        {
            IPomodoroManager manager = this as IPomodoroManager;
            Debug.Assert(manager != null);
            manager.StopSegment();

            BroadcastSegmentStatus(SegmentStatus.Complete);

            manager.GetNextSegement();
        }

        private void BroadcastSegmentProgress()
        {
            SegmentProgressChanged progress = new SegmentProgressChanged(_currentSegment);
            _events.PublishOnUIThread(progress);
        }

        private void BroadcastSegmentStatus(SegmentStatus status)
        {
            SegmentStatusChanged statusUpdate = new SegmentStatusChanged(status);
            _events.PublishOnUIThread(statusUpdate);
        }

        private void StopCurrentSegment()
        {
            _segmentTimer.Stop();

            if (_currentSegment.SegmentType == PomodoroSegmentType.Working)
            {
                RequestMediaChange(MediaRequestType.Stop);
            }

            BroadcastSegmentStatus(SegmentStatus.Stopped);
        }

        public void StartSegment()
        {
            _segmentTimer.Start();
            
            if (_currentSegment.SegmentType == PomodoroSegmentType.Working)
            {
                RequestMediaChange(MediaRequestType.Play);
            }
            BroadcastSegmentStatus(SegmentStatus.Running);
        }

        private void RequestMediaChange(MediaRequestType type)
        {
            MediaRequest request = new MediaRequest(type);
            _events.PublishOnUIThread(request);
        }

        public void StopSegment()
        {
            StopCurrentSegment();
        }

        public void RestartSegment()
        {
            StopCurrentSegment();
            _currentSegment.Progress = new TimeSpan();
            BroadcastSegmentProgress();
            if (_currentSegment.SegmentType == PomodoroSegmentType.Working)
            {
                RequestMediaChange(MediaRequestType.Restart);
            }
            StartSegment();
        }

        public void ResumeSegment()
        {
            StartSegment();
        }

        public void GetNextSegement()
        {
            _currentSegment = _factory.GetNextSegment(_currentSong);
            BroadcastSegmentProgress();
        }

        public void Handle(MediaStatusChanged message)
        {
            switch (message.CurrentStatus)
            {
                case PlayingStatus.ReadyToPlay:
                    _currentSong = message.CurrentSong;

                    if (_currentSegment == null)
                    {
                        GetNextSegement();
                    }
                    break;
                case PlayingStatus.Playing:
                    if (!_segmentTimer.Enabled)
                    {
                        StartSegment();
                    }
                    break;
                case PlayingStatus.Paused:
                    if (_segmentTimer.Enabled)
                    {
                        StopCurrentSegment();
                    }
                    break;
                case PlayingStatus.Completed:
                    CompleteSegment();
                    break;
                default:
                    if (_segmentTimer.Enabled && _currentSegment.SegmentType == PomodoroSegmentType.Working)
                    {
                        StopCurrentSegment();
                    }
                    break;
            }
        }

        public void Handle(SegmentRequest message)
        {
            switch (message.Action)
            {
                case SegmentRequestAction.StartNext:
                    if (_currentSegment.Progress.TotalSeconds == 0)
                    {
                        StartSegment();
                    }
                    else
                    {
                        GetNextSegement();
                    }
                    break;
                case SegmentRequestAction.Pause:
                    StopSegment();
                    break;
                case SegmentRequestAction.Restart:
                    RestartSegment();
                    break;
                case SegmentRequestAction.Resume:
                    ResumeSegment();
                    break;
                default:
                    throw new InvalidOperationException($"Unknown SegmentRequest Action: {message.Action}");
            }
        }

        public void Initialize()
        {
            if (string.IsNullOrEmpty(_settings.AudioFilePath))
            {
                throw new InvalidOperationException("Audio File Path Cannot Be Null");
            }

            BuildPlaylistFromFolderRequested request = new BuildPlaylistFromFolderRequested(_settings.AudioFilePath, _settings.Shuffle);
            _events.PublishOnUIThread(request);
        }

        public void Handle(MediaProgress message)
        {
            _currentSegment.Duration = message.Duration;
            _currentSegment.Progress = message.Progress;
        }
    }
}
