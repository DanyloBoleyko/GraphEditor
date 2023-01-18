using GraphEditorWPF.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphEditorWPF.Models.Context
{
    public class ContextParams
    {
        private Coordinates _coords = new Coordinates(0, 0);
        private object _source;

        public ContextParams(double x = 0, double y = 0, object source = null)
        {
            _source = source;
            _coords = new Coordinates(x, y);
        }

        public Coordinates Position
        {
            get { return _coords; }
            set { _coords = value; }
        }

        public object Source
        {
            get { return _source; }
            set { _source = value; }
        }
    }
}
