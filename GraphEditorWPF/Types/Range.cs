using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GraphEditorWPF.Types
{
    public class Range
    {
        private double _min = 0, _max = 0;

        public Range(double min = 0, double max = 0)
        {
            _min = min;
            _max = max;
        }

        public double Min
        {
            get { return _min; }
            set { _min = value; }
        }

        public double Max
        {
            get { return _max; }
            set { _max = value; }
        }
    }
}
