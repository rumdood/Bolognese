using Bolognese.Common.Configuration;
using Bolognese.Common.Media;
using Bolognese.Desktop;
using Caliburn.Micro;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

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
        IPlaylistBuilder _builder;
        IFileSystem _fileSystem;
        TrackManager _manager;
        Song _theSong;

        public MediaManagerTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        internal IEnumerable<Song> GetSongs()
        {
            var song1 = new Song(@"c:\foo\", "Song1", TimeSpan.FromSeconds(600));
            var song2 = new Song(@"c:\foo\", "Song2", TimeSpan.FromSeconds(300));
            var song3 = new Song(@"c:\foo\", "Song3", TimeSpan.FromSeconds(450));

            return new Song[] { song1, song2, song3 };
        }

        [TestInitialize]
        public void Initialize()
        {
            _theSong = new Song(@"c:\foo\", "Foo", TimeSpan.FromSeconds(60));
            _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\foo\Foo.mp3", new MockFileData("This won't work") }
            });
            _events = new Mock<IEventAggregator>();

            _factory = new Mock<ISongFactory>();

            _settings = new Mock<IConfigurationSettings>().SetupAllProperties();
            _settings.Object.AudioFilePath = @"D:\CloudStorage\OneDrive\Music\Carl Franklin\Music To Code By";
            _settings.Object.LongBreakCount = 4;
            _settings.Object.LongBreakDuration = 2;
            _settings.Object.ShortBreakDuration = 1;
            _settings.Object.PomodoroDuration = 1;
            _settings.Object.Shuffle = false;

            _manager = new TrackManager(_events.Object, 
                                        _settings.Object, 
                                        _fileSystem, 
                                        _factory.Object, 
                                        _builder);
        }

        internal int BuildPlaylist1(int[] lengths)
        {
            var target = 25;
            var fudgeFactor = 1;
            var currentLength = 0;

            int lowerBound = target - fudgeFactor;
            int upperBound = target + fudgeFactor;
            int maxRemainder = upperBound;

            int index = -1;

            while (++index < lengths.Length
                && maxRemainder >= (upperBound - lowerBound))
            {
                if (maxRemainder >= lengths[index])
                {
                    currentLength += lengths[index];
                    maxRemainder = upperBound - currentLength;
                }
            }

            return currentLength;
        }

        internal int BuildPlaylist2(int[] lengths)
        {
            var target = 25;
            var fudgeFactor = 1;
            var currentLength = 0;

            int lowerBound = target - fudgeFactor;
            int upperBound = target + fudgeFactor;
            int maxRemainder = upperBound;

            for (int i = 0; i < lengths.Length; i++)
            {
                if (lengths[i] > maxRemainder)
                {
                    continue;
                }

                currentLength += lengths[i];
                maxRemainder = upperBound - currentLength;

                if (maxRemainder <= (upperBound - lowerBound))
                {
                    break;
                }
            }

            return currentLength;
        }

        [TestMethod()]
        public void BuildPlaylistShouldFillTheBucket()
        {
            int[] lengths = { 12, 2, 15, 24, 7, 9, 4, 7 };

            var watch = System.Diagnostics.Stopwatch.StartNew();
            int result = BuildPlaylist1(lengths);
            watch.Stop();
            var e1 = watch.ElapsedTicks;

            watch = System.Diagnostics.Stopwatch.StartNew();
            int result2 = BuildPlaylist2(lengths);
            watch.Stop();
            var e2 = watch.ElapsedTicks;

            Assert.IsTrue(e1 > e2);
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
