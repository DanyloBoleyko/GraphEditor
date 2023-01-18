using GraphEditorWPF.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphEditorWPF.Models.EdgeModels
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EdgeParams
    {
        private Coordinates _from = new Coordinates(0, 0);
        private Coordinates _to = new Coordinates(0, 0);

        public EdgeParams(double fx = 0, double fy = 0, double tx = 0, double ty = 0)
        {
            _from.X = fx;
            _from.Y = fy;
            _to.X = tx;
            _to.Y = ty;
        }

        public Coordinates From
        {
            get { return _from; }
            set { _from = value; }
        }

        public Coordinates To
        {
            get { return _to; }
            set { _to = value; }
        }

        [JsonProperty("from_x")]
        public double FromX
        {
            get { return _from.X; }
            set { _from.X = value; }
        }

        [JsonProperty("from_y")]
        public double FromY
        {
            get { return _from.Y; }
            set { _from.Y = value; }
        }

        [JsonProperty("to_x")]
        public double ToX
        {
            get { return _to.X; }
            set { _to.X = value; }
        }

        [JsonProperty("to_y")]
        public double ToY
        {
            get { return _to.Y; }
            set { _to.Y = value; }
        }
    }
}
