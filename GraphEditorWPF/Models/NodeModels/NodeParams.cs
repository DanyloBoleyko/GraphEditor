using GraphEditorWPF.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphEditorWPF.Models.NodeModels
{
    [JsonObject(MemberSerialization.OptIn)]
    public class NodeParams
    {
        private Coordinates _coords = new Coordinates();
        private double _t = 0, _l = 0, _h = 0, _w = 0;

        public NodeParams(double x = 0, double y = 0, double top = 0, double left = 0, double height = 0, double width = 0)
        {
            _coords = new Coordinates(x, y);
            _t = top;
            _l = left;
            _h = height;
            _w = width;
        }

        public Coordinates Position
        {
            get { return _coords; }
            set { _coords = value; }
        }

        [JsonProperty("x")]
        public double X
        {
            get { return _coords.X; }
            set { _coords.X = value; }
        }

        [JsonProperty("y")]
        public double Y
        {
            get { return _coords.Y; }
            set { _coords.Y = value; }
        }

        public double Height
        {
            get { return _h; }
            set { _h = value; }
        }

        public double Width
        {
            get { return _w; }
            set { _w = value; }
        }

        public double Top
        {
            get { return _t; }
            set { _t = value; }
        }

        public double Left
        {
            get { return _l; }
            set { _l = value; }
        }
    }
}
