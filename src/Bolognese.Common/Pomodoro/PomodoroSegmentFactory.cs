using Bolognese.Common.Configuration;
using System;
using Bolognese.Common.Media;

namespace Bolognese.Common.Pomodoro
{
    public class PomodoroSegmentFactory : IPomodoroSegmentFactory
    {
        IConfigurationSettings _settings;
        PomodoroSegment _previousSegment;
        int _totalWorkSegments = 0;

        public PomodoroSegmentFactory(IConfigurationSettings settings)
        {
            _settings = settings;
        }

        public PomodoroSegment GetSegment(PomodoroSegmentType segmentType)
        {
            TimeSpan duration;
            switch (segmentType)
            {
                case PomodoroSegmentType.LongBreak:
                    duration = TimeSpan.FromMinutes(_settings.LongBreakDuration);
                    break;
                case PomodoroSegmentType.ShortBreak:
                    duration = TimeSpan.FromMinutes(_settings.ShortBreakDuration);
                    break;
                case PomodoroSegmentType.Working:
                    duration = TimeSpan.FromMinutes(_settings.PomodoroDuration);
                    break;
                default:
                    duration = TimeSpan.FromMinutes(25);
                    break;
            }

            PomodoroSegment segment = new PomodoroSegment()
            {
                SegmentType = segmentType,
                Duration = duration
            };

            return segment;
        }

        public PomodoroSegment GetNextSegment()
        {
            return GetNextSegment(new TimeSpan());
        }

        public PomodoroSegment GetNextSegment(TimeSpan durationOverride)
        {
            PomodoroSegment next;

            if (_previousSegment == null)
            {
                next = GetSegment(PomodoroSegmentType.Working);
            }
            else
            {
                switch (_previousSegment.SegmentType)
                {
                    case PomodoroSegmentType.LongBreak:
                    case PomodoroSegmentType.ShortBreak:
                        next = GetSegment(PomodoroSegmentType.Working);
                        break;
                    case PomodoroSegmentType.Working:
                        ++_totalWorkSegments;
                        int workSegments = _totalWorkSegments % _settings.LongBreakCount;
                        if (workSegments == 0)
                        {
                            next = GetSegment(PomodoroSegmentType.LongBreak);
                        }
                        else
                        {
                            next = GetSegment(PomodoroSegmentType.ShortBreak);
                        }
                        break;
                    default:
                        throw new InvalidOperationException("Invalid Segment Type Found in PomodoroFactory");
                }
            }

            if (next.SegmentType == PomodoroSegmentType.Working 
                && durationOverride.TotalSeconds > 0)
            {
                next.Duration = durationOverride;
            }

            _previousSegment = next;
            return next;
        }

        public PomodoroSegment GetNextSegment(Song song)
        {
            TimeSpan duration = song.Duration;
            return GetNextSegment(duration);
        }
    }
}
