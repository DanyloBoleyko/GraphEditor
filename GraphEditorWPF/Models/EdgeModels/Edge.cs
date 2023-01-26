using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GraphEditorWPF.EventArguments;
using GraphEditorWPF.Models.NodeModels;
using GraphEditorWPF.Types;
using GraphEditorWPF.Utils;
using Newtonsoft.Json;

namespace GraphEditorWPF.Models.EdgeModels
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Edge : Keyable
    {
        private double _width = 0;
        private double _value = 0;
        private EdgeParams _params = new EdgeParams();
        private Node _startNode;
        private Node _endNode;
        private string _startNodeKey;
        private string _endNodeKey;

        public event EventHandler<double> WeightChanged;

        public Edge(Node start = null, Node end = null, double width = 0.5)
        {
            _width = width;
            _startNode = start;
            _endNode = end;
            _params = new EdgeParams();
        }

        public double Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public double Weight
        {
            get { return _value; }
            set {
                var prev = _value;
                _value = value;

                if (WeightChanged != null && prev != _value)
                {
                    WeightChanged(this, _value);
                }
            }
        }

        [JsonProperty("params")]
        public EdgeParams Params
        {
            get { return _params; }
            set { _params = value; }
        }

        public Node StartNode
        {
            get { return _startNode; }
            set 
            { 
                _startNode = value;
                if (_startNode != null)
                {
                    _startNodeKey = _startNode.Key;
                }
            }
        }

        public Node EndNode
        {
            get { return _endNode; }
            set 
            { 
                _endNode = value; 
                if (_endNode != null)
                {
                    _endNodeKey = _endNode.Key;
                }
            }
        }

        [JsonProperty("from")]
        public string StartNodeKey
        {
            get { return _startNodeKey; }
            set { _startNodeKey = value; }
        }

        [JsonProperty("to")]
        public string EndNodeKey
        {
            get { return _endNodeKey; }
            set { _endNodeKey = value; }
        }

        [JsonProperty("weight")]
        public double Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public override string ToString()
        {
            if (_startNode == null || _endNode == null) return "";

            var result = StartNode.Label;

            result += string.Format(" - {0} -> ", _value, _width);

            result += EndNode.Label;
            result += ";";

            return result;
        }
    }
}
