using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bolognese.Desktop
{
    public class SegmentProgressChanged
    {
        private TimeSpan _progress;
        private TimeSpan _totalLength;

        public TimeSpan Progress
        {
            get { return _progress; }
        }

        public TimeSpan TotalTime
        {
            get { return _totalLength; }
        }

        public SegmentProgressChanged(TimeSpan total, TimeSpan progress)
        {
            _totalLength = total;
            _progress = progress;
        }
    }
}
