using System;

namespace Bolognese.Common.Pomodoro
{
    public class SegmentProgressChanged
    {
        private PomodoroSegment _segment;

        public PomodoroSegment Segment
        {
            get { return _segment; }
        }

        public SegmentProgressChanged(PomodoroSegment segment)
        {
            _segment = segment;
        }
    }
}
