using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using Caliburn.Micro;
using Bolognese.Common.Configuration;
using Bolognese.Common.Media;
using Bolognese.Desktop.Events;

namespace Bolognese.Desktop
{
    public class TrackManager : IMediaManager, 
                                IHandleWithTask<BuildPlaylistFromFolderRequested>, 
                                IHandle<MediaRequest>, 
                                IHandle<PlaylistReady>
    {
        private readonly IEventAggregator _events;
        private readonly ISongFactory _songFactory;
        private readonly IConfigurationSettings _configuration;
        private readonly Queue<Song> _songQueue = new Queue<Song>();
        private Song _currentSong;
        private MediaPlayer _player;
        private DispatcherTimer _timer;
        private PlayingStatus _status = PlayingStatus.Stopped;
        private readonly IFileSystem _fileSystem;
        private readonly IPlaylistBuilder _playlistBuilder;

        string IMediaManager.CurrentSongTitle
        {
            get
            {
                return _currentSong.Title;
            }
        }

        Queue<Song> IMediaManager.CurrentSongQueue
        {
            get
            {
                return _songQueue;
            }
        }

        private void ChangePlayingStatus(PlayingStatus status)
        {
            _status = status;

            if (_currentSong == null)
            {
                _events.PublishOnBackgroundThread(new MediaStatusChanged(_status));
            }
            else
            {
                _events.PublishOnBackgroundThread(new MediaStatusChanged(_status, _currentSong));
            }
        }

        public TrackManager(IEventAggregator events, 
                            IConfigurationSettings configuration, 
                            IFileSystem fileSystem,
                            ISongFactory songFactory,
                            IPlaylistBuilder builder)
        {
            _events = events;
            _events.Subscribe(this);
            _configuration = configuration;
            _fileSystem = fileSystem;
            _songFactory = songFactory;
            _playlistBuilder = builder;

            _player = new MediaPlayer();
            _player.MediaFailed += Player_MediaFailed;
            _player.MediaEnded += Player_MediaEnded;
            _player.MediaOpened += Player_MediaOpened;

            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_status == PlayingStatus.Playing)
            {
                MediaProgress progress = new MediaProgress(_player.NaturalDuration.TimeSpan, _player.Position);
                _events.PublishOnBackgroundThread(progress);
            }
        }

        private void Player_MediaOpened(object sender, EventArgs e)
        {
            ChangePlayingStatus(PlayingStatus.ReadyToPlay);
        }

        private void Player_MediaEnded(object sender, EventArgs e)
        {
            IMediaManager mgr = this as IMediaManager;
            mgr.Stop();
            ChangePlayingStatus(PlayingStatus.Completed);

            mgr.OpenNextSong();
        }

        private void Player_MediaFailed(object sender, ExceptionEventArgs e)
        {
            ChangePlayingStatus(PlayingStatus.Error);
        }

        void IMediaManager.Pause()
        {
            if (_player.CanPause)
            {
                _player.Pause();
                ChangePlayingStatus(PlayingStatus.Paused);
            }
        }

        void IMediaManager.PlayPlaylist(Playlist playlist)
        {
            foreach (Song s in playlist.Songs)
            {
                _songQueue.Enqueue(s);
            }

            IMediaManager mgr = this as IMediaManager;
            mgr.OpenNextSong();
        }

        void IMediaManager.Stop()
        {
            _player.Stop();
            ChangePlayingStatus(PlayingStatus.Stopped);
        }

        void IMediaManager.PlayOpenSong()
        {
            if (_currentSong != null && 
                (_status == PlayingStatus.Paused || _status == PlayingStatus.ReadyToPlay))
            {
                _player.Play();
                ChangePlayingStatus(PlayingStatus.Playing);
            }
        }

        void IMediaManager.OpenPlaylist(Playlist playlist)
        {
            IEnumerable<Song> songs = playlist.Songs;

            if (_configuration.Shuffle)
            {
                songs = playlist.Songs.OrderBy(x => Guid.NewGuid());
            }

            foreach (Song song in songs)
            {
                _songQueue.Enqueue(song);
            }
        }

        async Task IMediaManager.BuildPlaylist(string audioFilePath)
        {
            if (string.IsNullOrEmpty(audioFilePath)
                || !_fileSystem.Directory.Exists(audioFilePath))
            {
                throw new InvalidOperationException("Music Folder Does Not Exist");
            }

            Playlist current = await GeneratePlaylistFromFolder(audioFilePath);
            var ready = new PlaylistReady
            {
                Playlist = current
            };

            _events.PublishOnUIThread(ready);
        }

        private async Task<Playlist> GeneratePlaylistFromFolder(string audioFilePath)
        {
            var list = await Task.Run(() =>
            {
                var songs = _songFactory.GetSongs(audioFilePath);
                var rnd = new Random();
                var randomSongs = songs.OrderBy(x => rnd.Next());
                var playlist = _playlistBuilder.GeneratePlaylist(randomSongs, _configuration.PomodoroDuration, 2);

                return playlist;
            });

            return list;
        }

        private void OpenSong(Song song)
        {
            Uri songUri = new Uri(song.Path, UriKind.Absolute);
            _player.Open(songUri);
            _currentSong = song;
        }

        void IMediaManager.OpenNextSong()
        {
            if (_songQueue.Count == 0)
            {
                return;
            }

            Song nextSong = _songQueue.Dequeue();
            OpenSong(nextSong);
        }

        public async Task Handle(BuildPlaylistFromFolderRequested message)
        {
            IMediaManager manager = this as IMediaManager;
            Debug.Assert(manager != null);

            await manager.BuildPlaylist(message.FolderPath);
        }

        public void Handle(MediaRequest message)
        {
            IMediaManager manager = this as IMediaManager;
            Debug.Assert(manager != null);

            switch (message.RequestType)
            {
                case MediaRequestType.Play:
                    manager.PlayOpenSong();
                    break;
                case MediaRequestType.Pause:
                case MediaRequestType.Stop:
                    manager.Pause();
                    break;
                case MediaRequestType.Restart:
                    manager.Stop();
                    _player.Position = new TimeSpan();
                    break;
                default:
                    throw new InvalidOperationException("Unknown Media Request");
            }
        }

        void IHandle<PlaylistReady>.Handle(PlaylistReady message)
        {
            var trackManager = this as IMediaManager;
            trackManager.OpenPlaylist(message.Playlist);
            trackManager.OpenNextSong();
        }
    }
}