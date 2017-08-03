using Bolognese.Common.Configuration;
using Bolognese.Common.Media;
using Bolognese.Desktop;
using Caliburn.Micro;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;

namespace BolognesePlayerTests.MediaManager
{
    /// <summary>
    /// Summary description for MediaManager
    /// </summary>
    [TestClass]
    public class MediaManagerTests
    {
        const int SongDuration = 65;
        const int PomodoroDuration = 3;

        Mock<IConfigurationSettings> _settings;
        Mock<IEventAggregator> _events;
        Mock<ISongFactory> _factory;
        IFileSystem _fileSystem;
        TrackManager _manager;
        Song _theSong;

        public MediaManagerTests()
        {
        }

        private void SetupFileSystem()
        {
            _fileSystem = new MockFileSystem();
            var fsRoot = _fileSystem.Directory.CreateDirectory(@"C:\");
            var musicFolder = fsRoot.CreateSubdirectory(@"C:\Foo\");

            // create a bunch of files
            _fileSystem.File.Create(@"C:\Foo\Foo.mp3");
            _fileSystem.File.Create(@"C:\Foo\Foo2.mp3");
            _fileSystem.File.Create(@"C:\Foo\Foo3.mp3");
            _fileSystem.File.Create(@"C:\Foo\Foo4.mp3");
            _fileSystem.File.Create(@"C:\Foo\Foo5.mp3");
        }

        [TestInitialize]
        public void Initialize()
        {
            _theSong = new Song(@"c:\foo\", "Foo", TimeSpan.FromSeconds(SongDuration));
            _events = new Mock<IEventAggregator>();

            _factory = new Mock<ISongFactory>();
            _factory.Setup(factory => factory.GetSongFromFile(It.IsAny<FileInfoBase>())).Returns(_theSong);

            SetupFileSystem();

            _settings = new Mock<IConfigurationSettings>().SetupAllProperties();
            _settings.Object.AudioFilePath = @"C:\Foo\";
            _settings.Object.LongBreakCount = 4;
            _settings.Object.LongBreakDuration = 2;
            _settings.Object.ShortBreakDuration = 1;
            _settings.Object.PomodoroDuration = PomodoroDuration;
            _settings.Object.Shuffle = false;

            _manager = new TrackManager(_events.Object, _settings.Object, _fileSystem, _factory.Object);
        }

        [TestMethod()]
        public void SubscribeToEvents()
        {
            _events.Verify(x => x.Subscribe(_manager));
        }

        [TestMethod()]
        public async Task SongQueueIsUpToOneMinuteLongerThanPomodoroTime()
        {
            SubscribeToEvents();
            BuildPlaylistFromFolderRequested request = new BuildPlaylistFromFolderRequested(_settings.Object.AudioFilePath, true);
            await _manager.Handle(request);

            IMediaManager mgr = _manager as IMediaManager;
            int songCount = mgr.CurrentSongQueue.Count;
            int totalTime = songCount * SongDuration;
            int pomodoroTime = (PomodoroDuration * 60) + 60;

            Assert.IsTrue(totalTime <= pomodoroTime, $"Max allowable time is {pomodoroTime}, total time is {totalTime}");
        }
    }
}
