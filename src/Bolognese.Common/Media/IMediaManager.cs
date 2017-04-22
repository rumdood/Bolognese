using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bolognese.Common.Media
{
    public interface IMediaManager
    {
        string CurrentSongTitle { get; }
        Queue<Song> CurrentSongQueue { get; }
        void OpenPlaylist(Playlist playlist);
        void PlayPlaylist(Playlist playlist);
        void OpenNextSong();
        void PlayOpenSong();
        void Stop();
        void Pause();
        Task BuildPlaylist(string audioFilePath);
    }
}
