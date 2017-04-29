using Bolognese.Desktop.ViewModels;
using Bolognese.Common.Configuration;
using Bolognese.Common.Pomodoro;
using Caliburn.Micro;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Bolognese.Desktop.ViewModels.Tests
{
    [TestClass()]
    public class SmallPlayerViewModelTests
    {
        SmallPlayerViewModel _vm;
        Mock<IEventAggregator> _events;
        Mock<IPomodoroManager> _manager;
        Mock<IConfigurationSettings> _settings;

        [TestInitialize]
        public void Initialize()
        {
            _events = new Mock<IEventAggregator>();
            _manager = new Mock<IPomodoroManager>();
            _settings = new Mock<IConfigurationSettings>();

            _vm = new SmallPlayerViewModel(_events.Object, _manager.Object, _settings.Object);

            _manager.Setup(mgr => mgr.GetNextSegement())
                .Callback(() =>
                {
                    SegmentProgressChanged newStatus = new SegmentProgressChanged(
                        new PomodoroSegment()
                        {
                            Duration = TimeSpan.FromSeconds(60),
                            SegmentType = PomodoroSegmentType.Working,
                            Status = SegmentStatus.Running,
                            Progress = TimeSpan.FromSeconds(0)
                        });
                    _vm.Handle(newStatus);
                }
                );

            _manager.Setup(mgr => mgr.StopSegment())
                .Callback(() =>
                {
                    SegmentStatusChanged newStatus = new SegmentStatusChanged(SegmentStatus.Stopped);
                    _vm.Handle(newStatus);
                });

            _manager.Object.GetNextSegement();
        }

        [TestMethod()]
        public void SubscribeToEvents()
        {
            _events.Verify(x => x.Subscribe(_vm));
        }

        private double GetProgressPercentage(PomodoroSegmentType segmentType)
        {
            TimeSpan totalTime = TimeSpan.FromSeconds(60);
            TimeSpan currentPosition = TimeSpan.FromSeconds(18);

            SegmentProgressChanged songProgress = new SegmentProgressChanged(new PomodoroSegment()
            {
                SegmentType = segmentType,
                Duration = totalTime,
                Progress = currentPosition
            });

            _vm.Handle(songProgress);

            return _vm.CurrentSegmentProgress;
        }

        [TestMethod()]
        public void SegmentProgressShouldBeNegativeWhilePlaying()
        {
            SubscribeToEvents();
            PomodoroSegmentType type = PomodoroSegmentType.Working;

            double targetPercentage = 70;
            double actualPercentage = GetProgressPercentage(type);

            Assert.AreEqual(targetPercentage, actualPercentage, $"Progress percentage should be {targetPercentage}, got {actualPercentage}");
        }

        [TestMethod()]
        public void SegmentProgressShouldBePositiveWhileBreaking()
        {
            SubscribeToEvents();
            PomodoroSegmentType type = PomodoroSegmentType.ShortBreak;

            double targetPercentage = 30;
            double actualPercentage = GetProgressPercentage(type);

            Assert.AreEqual(targetPercentage, actualPercentage, $"Progress percentage should be {targetPercentage}, got {actualPercentage}");
        }

        [TestMethod()]
        public void CurrentStatusShouldChangeToReflectManagerChangedEvents()
        {
            SubscribeToEvents();
            SegmentStatusChanged status = new SegmentStatusChanged(SegmentStatus.Running);
            _vm.Handle(status);

            Assert.AreEqual(SegmentStatus.Running, _vm.CurrentSegment.Status);
            Assert.IsTrue(_vm.IsRunning);

            status = new SegmentStatusChanged(SegmentStatus.Stopped);
            _vm.Handle(status);

            Assert.AreEqual(SegmentStatus.Stopped, _vm.CurrentSegment.Status);
            Assert.IsFalse(_vm.IsRunning);
        }

        [TestMethod()]
        public void PlayPauseShouldPlayWhenPaused()
        {
            SubscribeToEvents();
            SegmentStatusChanged status = new SegmentStatusChanged(SegmentStatus.Stopped);
            _vm.Handle(status);

            Assert.AreEqual(SegmentStatus.Stopped, _vm.CurrentSegment.Status);

            _vm.PlayPause();
            _events
                .Verify(x => x.Publish(It.Is<SegmentRequest>(_ => _.Action == SegmentRequestAction.Resume), Execute.OnUIThread), Times.Once);
        }

        [TestMethod()]
        public void TimeRemainingShouldUpdate()
        {
            SubscribeToEvents();
            TimeSpan totalTime = TimeSpan.FromSeconds(60);
            TimeSpan currentPosition = TimeSpan.FromSeconds(18);

            SegmentProgressChanged segmentProgress = new SegmentProgressChanged(new PomodoroSegment()
            {
                Duration = totalTime,
                Progress = currentPosition
            });
            _vm.Handle(segmentProgress);

            TimeSpan target = TimeSpan.FromSeconds(42);
            Assert.AreEqual(target, _vm.TimeRemaining, "TimeRemaining should be 42 seconds");
        }

        [TestMethod()]
        public void CannotRestartWhenPlaying()
        {
            SubscribeToEvents();

            SegmentStatusChanged status = new SegmentStatusChanged(SegmentStatus.Running);
            _vm.Handle(status);

            bool canRestart = _vm.CanRestart;
            Assert.IsFalse(canRestart);
        }

        [TestMethod()]
        public void CanRestartWhenPausedAndProgressIsNotZero()
        {
            SubscribeToEvents();

            SegmentProgressChanged progress = new SegmentProgressChanged(new PomodoroSegment()
            {
                Duration = TimeSpan.FromSeconds(10),
                Progress = TimeSpan.FromSeconds(2),
                SegmentType = PomodoroSegmentType.Working,
                Status = SegmentStatus.Stopped
            });

            _vm.Handle(progress);

            bool canRestart = _vm.CanRestart;
            Assert.IsTrue(canRestart);
        }

        [TestMethod()]
        public void CannotRestartWhenProgressIsZero()
        {
            SubscribeToEvents();

            SegmentProgressChanged progress = new SegmentProgressChanged(new PomodoroSegment()
            {
                Duration = TimeSpan.FromSeconds(10),
                Progress = new TimeSpan(),
                SegmentType = PomodoroSegmentType.Working,
                Status = SegmentStatus.Stopped
            });

            _vm.Handle(progress);

            bool canRestart = _vm.CanRestart;
            Assert.IsFalse(canRestart);
        }

        [TestMethod()]
        public void RestartButtonShouldRequestRestart()
        {
            SubscribeToEvents();

            SegmentProgressChanged progress = new SegmentProgressChanged(new PomodoroSegment()
            {
                Duration = TimeSpan.FromSeconds(10),
                Progress = new TimeSpan(8),
                SegmentType = PomodoroSegmentType.Working,
                Status = SegmentStatus.Stopped
            });

            _vm.Handle(progress);
            _vm.RestartSegment();

            _events
                .Verify(x => x.Publish(It.Is<SegmentRequest>(_ => _.Action == SegmentRequestAction.Restart), Execute.OnUIThread), Times.Once);
        }

        [TestMethod()]
        public void PlayButtonShouldBeEnabledOnBreakStart()
        {
            SubscribeToEvents();

            SegmentProgressChanged progress = new SegmentProgressChanged(new PomodoroSegment()
            {
                Duration = TimeSpan.FromSeconds(10),
                Progress = new TimeSpan(),
                SegmentType = PomodoroSegmentType.ShortBreak,
                Status = SegmentStatus.ReadyToStart
            });

            _vm.Handle(progress);
            Assert.IsTrue(_vm.CanPlayPause);
        }

        [TestMethod()]
        public void PlayButtonShouldBeDisabledOnBreakRunning()
        {
            SubscribeToEvents();

            SegmentProgressChanged progress = new SegmentProgressChanged(new PomodoroSegment()
            {
                Duration = TimeSpan.FromSeconds(10),
                Progress = TimeSpan.FromSeconds(8),
                SegmentType = PomodoroSegmentType.ShortBreak,
                Status = SegmentStatus.Running
            });

            _vm.Handle(progress);
            Assert.IsFalse(_vm.CanPlayPause);
        }
    }
}