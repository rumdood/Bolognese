using Bolognese.Desktop.ViewModels;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Caliburn.Micro;

namespace Bolognese.Desktop.ViewModels.Tests
{
    [TestClass()]
    public class SmallPlayerViewModelTests
    {
        SmallPlayerViewModel _vm;
        Mock<IEventAggregator> _events;
        Mock<ITrackManager> _manager;

        [TestInitialize]
        public void Initialize()
        {
            _events = new Mock<IEventAggregator>();
            _manager = new Mock<ITrackManager>();

            _manager.Setup(mgr => mgr.PlayCurrentTrack())
                .Callback(() =>
                {
                    PlayerStatusChanged newStatus = new PlayerStatusChanged(Tracks.PlayingStatus.Playing);
                    _vm.Handle(newStatus);
                }
                );

            _manager.Setup(mgr => mgr.Pause())
                .Callback(() =>
                {
                    PlayerStatusChanged newStatus = new PlayerStatusChanged(Tracks.PlayingStatus.Paused);
                    _vm.Handle(newStatus);
                });

            _vm = new SmallPlayerViewModel(_events.Object, _manager.Object);
        }

        [TestMethod()]
        public void SubscribeToEvents()
        {
            _events.Verify(x => x.Subscribe(_vm));
        }

        private double GetProgressPercentage()
        {
            TimeSpan totalTime = TimeSpan.FromSeconds(60);
            TimeSpan currentPosition = TimeSpan.FromSeconds(18);

            SegmentProgressChanged songProgress = new SegmentProgressChanged(totalTime, currentPosition);
            _vm.Handle(songProgress);

            return _vm.CurrentSegmentProgress;
        }

        [TestMethod()]
        public void SegmentProgressShouldBePositiveWhilePlaying()
        {
            SubscribeToEvents();

            PlayerStatusChanged status = new PlayerStatusChanged(Tracks.PlayingStatus.Playing);
            _vm.Handle(status);

            double targetPercentage = 30;
            double actualPercentage = GetProgressPercentage();

            Assert.AreEqual(targetPercentage, actualPercentage, $"Progress percentage should be {targetPercentage}, got {actualPercentage}");
        }

        [TestMethod()]
        public void SegmentProgressShouldBeNegativeWhileBreaking()
        {
            SubscribeToEvents();
            PlayerStatusChanged status = new PlayerStatusChanged(Tracks.PlayingStatus.ShortBreak);
            _vm.Handle(status);

            double targetPercentage = 70;
            double actualPercentage = GetProgressPercentage();

            Assert.AreEqual(targetPercentage, actualPercentage, $"Progress percentage should be {targetPercentage}, got {actualPercentage}");
        }

        [TestMethod()]
        public void CurrentStatusShouldChangeToReflectManagerChangedEvents()
        {
            SubscribeToEvents();
            PlayerStatusChanged status = new PlayerStatusChanged(Tracks.PlayingStatus.Playing);
            _vm.Handle(status);

            Assert.AreEqual(Tracks.PlayingStatus.Playing, _vm.CurrentStatus);
            Assert.IsTrue(_vm.IsPlaying);

            status = new PlayerStatusChanged(Tracks.PlayingStatus.ShortBreak);
            _vm.Handle(status);

            Assert.AreEqual(Tracks.PlayingStatus.ShortBreak, _vm.CurrentStatus);
            Assert.IsFalse(_vm.IsPlaying);
        }

        [TestMethod()]
        public void PlayPauseShouldPlayWhenPaused()
        {
            SubscribeToEvents();
            PlayerStatusChanged status = new PlayerStatusChanged(Tracks.PlayingStatus.Paused);
            _vm.Handle(status);

            Assert.AreEqual(Tracks.PlayingStatus.Paused, _vm.CurrentStatus);

            _vm.PlayPause();

            Assert.AreEqual(Tracks.PlayingStatus.Playing, _vm.CurrentStatus);
        }
    }
}