using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bolognese.Desktop.Tracks
{
    public enum PlayingStatus
    {
        Playing,
        Paused,
        Stopped,
        ShortBreak,
        LongBreak,
        ReadyToPlay,
        Error
    }
}
