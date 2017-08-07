using Bolognese.Common.Configuration;
using Bolognese.Common.Media;
using Bolognese.Desktop;
using Caliburn.Micro;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;

namespace BolognesePlayerTests.MediaManager
{
    /// <summary>
    /// Summary description for MediaManager
    /// </summary>
    [TestClass]
    public class MediaManagerTests
    {
        Mock<IConfigurationSettings> _settings;
        Mock<IEventAggregator> _events;
        Mock<ISongFactory> _factory;
        TrackManager _manager;
        Song _theSong;

        public MediaManagerTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [TestInitialize]
        public void Initialize()
        {
            _theSong = new Song(@"c:\foo\", "Foo", TimeSpan.FromSeconds(60));
            _events = new Mock<IEventAggregator>();

            _factory = new Mock<ISongFactory>();
            _factory.Setup(factory => factory.GetSongFromFile(It.IsAny<FileInfo>())).Returns(_theSong);

            _settings = new Mock<IConfigurationSettings>().SetupAllProperties();
            _settings.Object.AudioFilePath = @"D:\CloudStorage\OneDrive\Music\Carl Franklin\Music To Code By";
            _settings.Object.LongBreakCount = 4;
            _settings.Object.LongBreakDuration = 2;
            _settings.Object.ShortBreakDuration = 1;
            _settings.Object.PomodoroDuration = 1;
            _settings.Object.Shuffle = false;

            _manager = new TrackManager(_events.Object, _settings.Object, _factory.Object);
        }

        [TestMethod()]
        public void SubscribeToEvents()
        {
            _events.Verify(x => x.Subscribe(_manager));
        }

        [TestMethod]
        public async void BuildPlaylistShouldReturnSuccessfullyWithRealFiles()
        {
            SubscribeToEvents();
            BuildPlaylistFromFolderRequested request = new BuildPlaylistFromFolderRequested(_settings.Object.AudioFilePath, true);
            await _manager.Handle(request);

            IMediaManager mgr = _manager as IMediaManager;
            int target = 8;
            int actual = mgr.CurrentSongQueue.Count;
            Assert.AreEqual(target, actual, $"Expected {target}, got {actual}");
        }
    }
}
