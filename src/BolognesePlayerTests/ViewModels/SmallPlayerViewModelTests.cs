using Bolognese.Desktop.ViewModels;
using Bolognese.Common.Media;
using Bolognese.Common.Pomodoro;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Caliburn.Micro;
using Bolognese.Common.Configuration;

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

            _manager.Setup(mgr => mgr.StartNextSegment())
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

            _manager.Object.StartNextSegment();
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
            Assert.IsTrue(_vm.IsPlaying);

            status = new SegmentStatusChanged(SegmentStatus.Stopped);
            _vm.Handle(status);

            Assert.AreEqual(SegmentStatus.Stopped, _vm.CurrentSegment.Status);
            Assert.IsFalse(_vm.IsPlaying);
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

            SegmentProgressChanged songProgress = new SegmentProgressChanged(new PomodoroSegment()
            {
                Duration = totalTime,
                Progress = currentPosition
            });
            _vm.Handle(songProgress);

            TimeSpan target = TimeSpan.FromSeconds(42);
            Assert.AreEqual(target, _vm.TimeRemaining, "TimeRemaining should be 42 seconds");
        }
    }
}