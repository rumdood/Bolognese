using Bolognese.Desktop.Tracks;
using System.IO;

namespace Bolognese.Desktop
{
    public interface ISongFactory
    {
        Song GetSongFromFile(FileInfo file);
    }
}
