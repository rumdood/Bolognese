using System;

namespace Bolognese.Common.Media
{
    public class Song
    {
        public string FilePath { get; private set; }
        public string Title { get; private set; }
        public TimeSpan Duration { get; private set; }

        public Song(string filePath, string name, TimeSpan duration)
        {
            FilePath = filePath;
            Title = name;
            Duration = duration;
        }

        public void AdjustDuration(TimeSpan newDuration)
        {
            Duration = newDuration;
        }
    }
}
