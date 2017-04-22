using System;
using System.Collections.Generic;
using System.Text;

namespace Bolognese.Common.Media
{
    public class MediaProgress
    {
        public TimeSpan Duration { get; private set; }
        public TimeSpan Progress { get; private set; }

        public MediaProgress(TimeSpan duration, TimeSpan progress)
        {
            Duration = duration;
            Progress = progress;
        }
    }
}
