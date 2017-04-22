using System;

namespace Bolognese.Common.Pomodoro
{
    public class PomodoroSegment
    {
        TimeSpan _progress = new TimeSpan();
        SegmentStatus _status = SegmentStatus.ReadyToStart;

        public PomodoroSegmentType SegmentType { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
            }
        }

        public SegmentStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
            }
        }
    }
}
