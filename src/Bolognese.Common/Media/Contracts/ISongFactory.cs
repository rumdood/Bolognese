using System;
using System.Collections.Generic;

namespace Bolognese.Common.Media
{
    public interface ISongFactory
    {
        Song GetSong(string uri);
        IEnumerable<Song> GetSongs(string uri);
    }
}
