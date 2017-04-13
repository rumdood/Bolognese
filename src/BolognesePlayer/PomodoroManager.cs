using System;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Windows.Media;
using Caliburn.Micro;
using Bolognese.Desktop.Tracks;
using System.IO;
using System.Threading.Tasks;

namespace Bolognese.Desktop
{
    class PomodoroManager : ITrackManager
    {
        private readonly IEventAggregator _events;
        private readonly ISongFactory _songFactory;
        private readonly Queue<Song> _songQueue = new Queue<Song>();
        private DispatcherTimer _songTimer;
        private Song _currentSong;
        private MediaPlayer _player;
        private PlayingStatus _status = PlayingStatus.Stopped;
        private TimeSpan _currentBreakTime;
        private int _pomodorosSinceBigBreak;
        private IConfigurationSettings _settings;
        private double _shortBreakDuration;
        private double _longBreakDuration;

        string ITrackManager.CurrentSongTitle
        {
            get
            {
                return _currentSong.Title;
            }
        }

        private void ChangePlayingStatus(PlayingStatus status)
        {
            _status = status;

            if (_currentSong == null
                || _status == PlayingStatus.LongBreak 
                || _status == PlayingStatus.ShortBreak)
            {
                _events.PublishOnUIThread(new PlayerStatusChanged(_status));
            }
            else
            {
                _events.PublishOnUIThread(new PlayerStatusChanged(_status, _currentSong.Title));
            }
        }

        public PomodoroManager(IEventAggregator events, ISongFactory songFactory)
        {
            _events = events;
            _songFactory = songFactory;

            _settings = BologneseConfigurationSettings.GetConfigurationSettings();
            _shortBreakDuration = TimeSpan.FromMinutes(_settings.ShortBreakDuration).TotalSeconds;
            _longBreakDuration = TimeSpan.FromMinutes(_settings.LongBreakDuration).TotalSeconds;

            _songTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _songTimer.Tick += SongTimer_Tick; ;

            _player = new MediaPlayer();
            _player.MediaFailed += Player_MediaFailed;
            _player.MediaEnded += Player_MediaEnded;
            _player.MediaOpened += Player_MediaOpened;
        }

        private void SongTimer_Tick(object sender, EventArgs e)
        {
            ITrackManager mgr = this as ITrackManager;

            if (_status == PlayingStatus.LongBreak ||
                _status == PlayingStatus.ShortBreak ||
                _status == PlayingStatus.Playing)
            {
                TimeSpan segmentTotal = new TimeSpan();
                TimeSpan progressTotal = new TimeSpan();

                switch (_status)
                {
                    case PlayingStatus.Playing:
                        segmentTotal = _player.NaturalDuration.TimeSpan;
                        progressTotal = _player.Position;
                        break;
                    case PlayingStatus.ShortBreak:
                        segmentTotal = TimeSpan.FromSeconds(_shortBreakDuration);
                        _currentBreakTime = _currentBreakTime.Add(_songTimer.Interval);
                        progressTotal = _currentBreakTime;

                        if (_currentBreakTime.TotalSeconds >= _shortBreakDuration)
                        {
                            ChangePlayingStatus(PlayingStatus.ReadyToPlay);
                        }

                        break;
                    case PlayingStatus.LongBreak:
                        segmentTotal = TimeSpan.FromSeconds(_longBreakDuration);
                        _currentBreakTime = _currentBreakTime.Add(_songTimer.Interval);
                        progressTotal = _currentBreakTime;

                        if (_currentBreakTime.TotalSeconds >= _longBreakDuration)
                        {
                            ChangePlayingStatus(PlayingStatus.ReadyToPlay);
                        }

                        break;
                    default:
                        // how the hell did we get HERE?
                        break;
                }

                SegmentProgressChanged changedEvent = new SegmentProgressChanged(segmentTotal, progressTotal);
                _events.PublishOnUIThread(changedEvent);
            }
        }

        private void Player_MediaOpened(object sender, EventArgs e)
        {
            _player.Play();

            if (!_songTimer.IsEnabled)
            {
                _songTimer.Start();
            }

            ChangePlayingStatus(PlayingStatus.Playing);
        }

        private void Player_MediaEnded(object sender, EventArgs e)
        {
            ITrackManager mgr = this as ITrackManager;
            mgr.Stop();

            _currentBreakTime = new TimeSpan();

            if (_pomodorosSinceBigBreak < _settings.LongBreakCount)
            {
                ChangePlayingStatus(PlayingStatus.ShortBreak);
                _pomodorosSinceBigBreak++;
            }
            else
            {
                ChangePlayingStatus(PlayingStatus.LongBreak);
                _pomodorosSinceBigBreak = 0;
            }
        }

        private void Player_MediaFailed(object sender, ExceptionEventArgs e)
        {
            ChangePlayingStatus(PlayingStatus.Error);
        }

        void ITrackManager.Pause()
        {
            if (_player.CanPause)
            {
                _player.Pause();
                ChangePlayingStatus(PlayingStatus.Paused);
            }
        }

        void ITrackManager.PlayNextTrack()
        {
            ITrackManager mgr = this as ITrackManager;

            mgr.Stop();

            if (_songQueue.Count == 0)
            {
                return;
            }

            Song nextSong = _songQueue.Dequeue();
            mgr.PlayTrack(nextSong);
        }

        void ITrackManager.PlayPlaylist(Playlist playlist)
        {
            foreach (Song s in playlist.Songs)
            {
                _songQueue.Enqueue(s);
            }

            ITrackManager mgr = this as ITrackManager;
            mgr.PlayNextTrack();
        }

        void ITrackManager.PlayTrack(Song song)
        {
            Uri songUri = new Uri(song.FilePath, UriKind.Absolute);
            _player.Open(songUri);
            _currentSong = song;
        }

        void ITrackManager.Stop()
        {
            _player.Stop();
            //_songTimer.Stop();

            ChangePlayingStatus(PlayingStatus.ReadyToPlay);
        }

        void ITrackManager.PlayCurrentTrack()
        {
            if (_currentSong != null && 
                (_status == PlayingStatus.Paused || _status == PlayingStatus.ReadyToPlay))
            {
                _player.Play();
                ChangePlayingStatus(PlayingStatus.Playing);
            }
        }

        void ITrackManager.OpenPlaylist(Playlist playlist)
        {
            foreach (Song s in playlist.Songs)
            {
                _songQueue.Enqueue(s);
            }
            ChangePlayingStatus(PlayingStatus.ReadyToPlay);
        }

        async Task ITrackManager.BuildPlaylist()
        {
            if (_settings.AudioFilePath == string.Empty
                || !Directory.Exists(_settings.AudioFilePath))
            {
                throw new InvalidOperationException("Music Folder Does Not Exist");
            }

            Playlist current = await GeneratePlaylistFromFolder();

            var trackManager = this as ITrackManager;
            trackManager.OpenPlaylist(current);

            PlayerStatusChanged status = new PlayerStatusChanged(PlayingStatus.ReadyToPlay);
            _events.PublishOnUIThread(status);
        }

        private async Task<Playlist> GeneratePlaylistFromFolder()
        {
            var list = await Task.Run(() =>
            {
                Playlist playlist = new Playlist();
                playlist.Name = _settings.AudioFilePath;
                DirectoryInfo directory = new DirectoryInfo(_settings.AudioFilePath);

                foreach (FileInfo file in directory.GetFiles("*.mp3"))
                {
                    Song song = _songFactory.GetSongFromFile(file);
                    playlist.Songs.Add(song);
                }

                return playlist;
            });

            return list;
        }
    }
}