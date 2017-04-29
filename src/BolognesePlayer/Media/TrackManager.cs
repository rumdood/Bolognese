using System;
using System.Collections.Generic;
using System.Windows.Media;
using Caliburn.Micro;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Bolognese.Common.Media;
using System.Diagnostics;
using System.Windows.Threading;
using Bolognese.Common.Configuration;

namespace Bolognese.Desktop
{
    public class TrackManager : IMediaManager, IHandleWithTask<BuildPlaylistFromFolderRequested>, IHandle<MediaRequest>
    {
        private readonly IEventAggregator _events;
        private readonly ISongFactory _songFactory;
        private readonly IConfigurationSettings _configuration;
        private readonly Queue<Song> _songQueue = new Queue<Song>();
        private Song _currentSong;
        private MediaPlayer _player;
        private DispatcherTimer _timer;
        private PlayingStatus _status = PlayingStatus.Stopped;

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

        public TrackManager(IEventAggregator events, IConfigurationSettings configuration, ISongFactory songFactory)
        {
            _events = events;
            _events.Subscribe(this);
            _configuration = configuration;
            _songFactory = songFactory;

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
            //_player.Position = _player.NaturalDuration.TimeSpan.Subtract(TimeSpan.FromSeconds(5));
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
                //_player.Position = _player.NaturalDuration.TimeSpan.Subtract(TimeSpan.FromSeconds(5));
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
                || !Directory.Exists(audioFilePath))
            {
                throw new InvalidOperationException("Music Folder Does Not Exist");
            }

            Playlist current = await GeneratePlaylistFromFolder(audioFilePath);

            var trackManager = this as IMediaManager;
            trackManager.OpenPlaylist(current);
            trackManager.OpenNextSong();
        }

        private async Task<Playlist> GeneratePlaylistFromFolder(string audioFilePath)
        {
            var list = await Task.Run(() =>
            {
                Playlist playlist = new Playlist();
                playlist.Name = audioFilePath;
                DirectoryInfo directory = new DirectoryInfo(audioFilePath);

                foreach (FileInfo file in directory.GetFiles("*.mp3"))
                {
                    Song song = _songFactory.GetSongFromFile(file);
                    playlist.Songs.Add(song);
                }

                return playlist;
            });

            return list;
        }

        private void OpenSong(Song song)
        {
            Uri songUri = new Uri(song.FilePath, UriKind.Absolute);
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
    }
}