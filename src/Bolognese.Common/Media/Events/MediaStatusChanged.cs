namespace Bolognese.Common.Media
{
    public class MediaStatusChanged
    {
        private PlayingStatus _status = PlayingStatus.Stopped;
        private Song _currentSong;

        public PlayingStatus CurrentStatus
        {
            get { return _status; }
        }

        public Song CurrentSong
        {
            get { return _currentSong; }
        }

        public MediaStatusChanged(PlayingStatus status)
        {
            _status = status;
        }

        public MediaStatusChanged(PlayingStatus status, Song song)
        {
            _status = status;
            _currentSong = song;
        }
    }
}
