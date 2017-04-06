﻿using System;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Windows.Media;
using Caliburn.Micro;
using Bolognese.Desktop.Tracks;

namespace Bolognese.Desktop
{
    class TrackManager : ITrackManager
    {
        private readonly IEventAggregator _events;
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
                return _currentSong.Name;
            }
        }

        private void ChangePlayingStatus(PlayingStatus status)
        {
            _status = status;
            _events.PublishOnUIThread(new PlayerStatusChanged(_status));
        }

        public TrackManager(IEventAggregator events)
        {
            _events = events;

            _settings = BologneseConfigurationSettings.GetConfigurationSettings();
            _shortBreakDuration = TimeSpan.FromMinutes(_settings.ShortBreakDuration).TotalSeconds;
            _longBreakDuration = TimeSpan.FromMinutes(_settings.LongBreakDuration).TotalSeconds;

            _songTimer = new DispatcherTimer();
            _songTimer.Interval = TimeSpan.FromMilliseconds(200);
            _songTimer.Tick += SongTimer_Tick; ;

            _player = new MediaPlayer();
            _player.MediaFailed += Player_MediaFailed;
            _player.MediaEnded += Player_MediaEnded;
            _player.MediaOpened += Player_MediaOpened;
        }

        private void SongTimer_Tick(object sender, EventArgs e)
        {
            double statusValue = 0;
            ITrackManager mgr = this as ITrackManager;

            if (_status == PlayingStatus.LongBreak ||
                _status == PlayingStatus.ShortBreak ||
                _status == PlayingStatus.Playing)
            {
                switch (_status)
                {
                    case PlayingStatus.Playing:
                        statusValue = (_player.Position.TotalSeconds / _player.NaturalDuration.TimeSpan.TotalSeconds) * 100;
                        break;
                    case PlayingStatus.ShortBreak:
                        _currentBreakTime = _currentBreakTime.Add(TimeSpan.FromMilliseconds(200));

                        if (_currentBreakTime.TotalSeconds >= _shortBreakDuration)
                        {
                            ChangePlayingStatus(PlayingStatus.ReadyToPlay);
                        }
                        else
                        {
                            statusValue = (_shortBreakDuration - _currentBreakTime.TotalSeconds) / _shortBreakDuration * 100;
                        }
                        break;
                    case PlayingStatus.LongBreak:
                        _currentBreakTime = _currentBreakTime.Add(TimeSpan.FromMilliseconds(200));

                        if (_currentBreakTime.TotalSeconds >= _longBreakDuration)
                        {
                            ChangePlayingStatus(PlayingStatus.ReadyToPlay);
                        }
                        else
                        {
                            statusValue = (_longBreakDuration - _currentBreakTime.TotalSeconds) / _longBreakDuration * 100;
                        }
                        break;
                    default:
                        // how the hell did we get HERE?
                        break;
                }

                _events.PublishOnUIThread(new SegmentProgressChanged(statusValue));
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

            ChangePlayingStatus(PlayingStatus.Stopped);
        }

        void ITrackManager.PlayCurrentTrack()
        {
            if (_currentSong != null && 
                (_status == PlayingStatus.Paused || _status == PlayingStatus.Stopped))
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
    }
}