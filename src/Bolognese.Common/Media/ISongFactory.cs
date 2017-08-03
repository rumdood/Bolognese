using System.IO.Abstractions;

namespace Bolognese.Common.Media
{
    public interface ISongFactory
    {
        Song GetSongFromFile(FileInfoBase file);
    }
}
