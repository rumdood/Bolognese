using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bolognese.Desktop.Tracks
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
