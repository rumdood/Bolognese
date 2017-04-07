using Bolognese.Desktop.Tracks;

namespace Bolognese.Desktop
{
    public class PlayerStatusChanged
    {
        private PlayingStatus _status = PlayingStatus.Stopped;
        private string _currentTrackTitle = string.Empty;

        public PlayingStatus CurrentStatus
        {
            get { return _status; }
        }

        public string CurrentTrackTitle
        {
            get { return _currentTrackTitle; }
        }

        public PlayerStatusChanged(PlayingStatus status)
        {
            _status = status;
        }

        public PlayerStatusChanged(PlayingStatus status, string trackTitle)
        {
            _status = status;
            _currentTrackTitle = trackTitle;
        }
    }
}
