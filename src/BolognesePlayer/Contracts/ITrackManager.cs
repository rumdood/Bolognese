using Bolognese.Desktop.Tracks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bolognese.Desktop
{
    public interface ITrackManager
    {
        string CurrentSongTitle { get; }
        void OpenPlaylist(Playlist playlist);
        void PlayPlaylist(Playlist playlist);
        void PlayTrack(Song song);
        void PlayNextTrack();
        void PlayCurrentTrack();
        void Stop();
        void Pause();
    }
}
