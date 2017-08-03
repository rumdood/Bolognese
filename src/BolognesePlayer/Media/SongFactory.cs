using TagLib;
using Bolognese.Common.Media;
using System;
using System.IO.Abstractions;

namespace Bolognese.Desktop
{
    public class SongFactory : ISongFactory
    {
        Song ISongFactory.GetSongFromFile(FileInfoBase file)
        {
            string filePath = file.FullName;
            TagLib.File tagFile = TagLib.File.Create(filePath);
            string title = string.Empty;
            TimeSpan duration = TimeSpan.FromSeconds(0);

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

                duration = tagFile.Properties.Duration;
            }

            Song song = new Song(file.FullName, title, duration);
            return song;
        }
    }
}
