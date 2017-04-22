using Bolognese.Common.Media;
using System;

namespace Bolognese.Common.Pomodoro
{
    public interface IPomodoroSegmentFactory
    {
        PomodoroSegment GetNextSegment();
        PomodoroSegment GetNextSegment(TimeSpan durationOverride);
        PomodoroSegment GetNextSegment(Song song);
        PomodoroSegment GetSegment(PomodoroSegmentType segmentType);
    }
}