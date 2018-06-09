using System;
using System.IO;

namespace Bolognese.Common.Media
{
    public interface ISongFactory
    {
        Song GetSongFromFile(FileInfo file);
    }
}
