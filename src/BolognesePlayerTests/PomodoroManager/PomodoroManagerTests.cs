using Bolognese.Common.Configuration;
using Bolognese.Common.Media;
using Bolognese.Common.Pomodoro;
using Bolognese.Desktop;
using Caliburn.Micro;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace BolognesePlayerTests.PomodoroManager
{
    [TestClass]
    public class PomodoroManagerTests
    {
        Mock<IConfigurationSettings> _settings;
        Mock<IEventAggregator> _events;
        Mock<IMediaManager> _media;
        Mock<IPomodoroSegmentFactory> _factory;
        Song _song;
        Bolognese.Common.Pomodoro.PomodoroManager _manager;

        [TestInitialize]
        public void Initialize()
        {
            _events = new Mock<IEventAggregator>();

            _settings = new Mock<IConfigurationSettings>().SetupAllProperties();
            _settings.Object.AudioFilePath = @"D:\CloudStorage\OneDrive\Music\Carl Franklin\Music To Code By";
            _settings.Object.LongBreakCount = 4;
            _settings.Object.LongBreakDuration = 2;
            _settings.Object.ShortBreakDuration = 1;
            _settings.Object.PomodoroDuration = 1;
            _settings.Object.Shuffle = false;

            _factory = new Mock<IPomodoroSegmentFactory>();

            _song = new Song("foo", "TestSong", TimeSpan.FromSeconds(10));

            _manager = new Bolognese.Common.Pomodoro.PomodoroManager(_settings.Object, _factory.Object, _events.Object);
        }

        private void SubscribeToEvents()
        {
            _events.Verify(x => x.Subscribe(_manager));
        }

        [TestMethod()]
        public void InitializeSendsAPlaylistRequest()
        {
            SubscribeToEvents();
            _manager.Initialize();

            _events
                .Verify(x => x.Publish(It.Is<BuildPlaylistFromFolderRequested>(
                                       _ => _.FolderPath.Equals(_settings.Object.AudioFilePath)), 
                                       Execute.OnUIThread)
                        , Times.Once);
        }

        [TestMethod()]
        public void MediaReadyToPlayEmitsSegmentChange()
        {
            SubscribeToEvents();

            _factory.Setup(x => x.GetNextSegment(_song)).Returns(new PomodoroSegment()
            {
                Duration = TimeSpan.FromSeconds(10),
                Progress = new TimeSpan(),
                SegmentType = PomodoroSegmentType.Working,
                Status = SegmentStatus.ReadyToStart
            });

            MediaStatusChanged media = new MediaStatusChanged(PlayingStatus.ReadyToPlay, _song);

            _manager.Handle(media);

            _events
                .Verify(x => x.Publish(It.Is<SegmentProgressChanged>(_ => _.Segment.SegmentType == PomodoroSegmentType.Working), Execute.OnUIThread), Times.Once);
        }
    }
}
