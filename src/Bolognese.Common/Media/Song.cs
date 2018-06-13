using System;

namespace Bolognese.Common.Media
{
    public class Song
    {
        public string Path { get; private set; }
        public string Title { get; private set; }
        public TimeSpan Duration { get; private set; }

        public Song(string path, string name, TimeSpan duration)
        {
            Path = path;
            Title = name;
            Duration = duration;
        }

        public void AdjustDuration(TimeSpan newDuration)
        {
            Duration = newDuration;
        }
    }
}
