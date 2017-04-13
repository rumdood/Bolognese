using System.IO;
using TagLib;
using Bolognese.Desktop.Tracks;

namespace Bolognese.Desktop
{
    public class SongFactory : ISongFactory
    {
        Song ISongFactory.GetSongFromFile(FileInfo file)
        {
            string filePath = file.FullName;
            TagLib.File tagFile = TagLib.File.Create(filePath);
            string title = string.Empty;

            using (tagFile)
            {
                Tag tag = tagFile.Tag;
                
                if (string.IsNullOrEmpty(tag.Title))
                {
                    title = file.Name;
                }
                else
                {
                    title = tag.Title;
                }
            }

            Song song = new Song(file.FullName, title);
            return song;
        }
    }
}
