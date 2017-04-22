using System.Collections.Generic;

namespace Bolognese.Common.Media
{
    public class Playlist
    {
        private readonly List<Song> _songs = new List<Song>();

        public IList<Song> Songs
        {
            get { return _songs; }
        }

        public string Name { get; set; }
    }
}
