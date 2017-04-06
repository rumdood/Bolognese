using Bolognese.Desktop.Tracks;

namespace Bolognese.Desktop
{
    public class PlayerStatusChanged
    {
        private PlayingStatus _status = PlayingStatus.Stopped;

        public PlayingStatus CurrentStatus
        {
            get { return _status; }
        }

        public PlayerStatusChanged(PlayingStatus status)
        {
            _status = status;
        }
    }
}
