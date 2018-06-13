using TagLib;
using Bolognese.Common.Media;
using System;
using System.IO.Abstractions;
using System.Collections.Generic;

namespace Bolognese.Desktop
{
    public class FileSystemSongFactory : ISongFactory
    {
        private readonly IFileSystem _fileSystem;

        public FileSystemSongFactory(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        Song ISongFactory.GetSong(string uri)
        {
            var file = _fileSystem.FileInfo.FromFileName(uri);
            var song = GetSongFromFile(file);
            return song;
        }

        IEnumerable<Song> ISongFactory.GetSongs(string uri)
        {
            var folder = _fileSystem.DirectoryInfo.FromDirectoryName(uri);
            var songs = GetSongsFromFolder(folder);
            return songs;
        }

        internal IEnumerable<Song> GetSongsFromFolder(DirectoryInfoBase folder)
        {
            var songs = new List<Song>();

            foreach (var file in folder.GetFiles("*.mp3"))
            {
                Song song = GetSongFromFile(file);
                songs.Add(song);
            }

            return songs;
        }

        internal Song GetSongFromFile(FileInfoBase file)
        {
            string filePath = file.FullName;
            File tagFile = File.Create(filePath);
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
