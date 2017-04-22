using System;
using System.Collections.Generic;
using System.Text;

namespace Bolognese.Common.Pomodoro
{
    public class SegmentStatusChanged
    {
        public SegmentStatus Status { get; private set; }

        public SegmentStatusChanged(SegmentStatus status)
        {
            Status = status;
        }
    }
}
