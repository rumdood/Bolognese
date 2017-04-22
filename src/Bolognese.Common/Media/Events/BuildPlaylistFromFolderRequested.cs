using System;
using System.Collections.Generic;
using System.Text;

namespace Bolognese.Common.Media
{
    public class BuildPlaylistFromFolderRequested
    {
        public string FolderPath { get; private set; }
        public bool Shuffle { get; private set; }

        public BuildPlaylistFromFolderRequested(string folderPath, bool shuffle)
        {
            FolderPath = folderPath;
            Shuffle = shuffle;
        }
    }
}
