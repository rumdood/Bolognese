using System;
using System.Collections.Generic;
using System.Text;

namespace Bolognese.Common.Media
{
    public interface IPlaylistBuilder
    {
        Playlist GeneratePlaylist(IEnumerable<Song> songs, 
                                  int maxTime, 
                                  int fudgeFactor);
    }
}
