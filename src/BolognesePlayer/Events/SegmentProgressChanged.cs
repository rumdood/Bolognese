using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bolognese.Desktop
{
    public class SegmentProgressChanged
    {
        private double _progress = 0;

        public double Progress
        {
            get { return _progress; }
        }

        public SegmentProgressChanged(double progress)
        {
            _progress = progress;
        }
    }
}
