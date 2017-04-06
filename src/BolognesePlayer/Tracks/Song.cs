using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bolognese.Desktop.Tracks
{
    public class Song
    {
        public string FilePath { get; set; }
        public string Name { get; set; }

        public Song(string filePath, string name)
        {
            FilePath = filePath;
            Name = name;
        }
    }
}
