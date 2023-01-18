using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using GraphEditorWPF.Types;
using GraphEditorWPF.Utils;
using GraphEditorWPF.Models.EdgeModels;
using Newtonsoft.Json;

namespace GraphEditorWPF.Models.NodeModels
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Node : Keyable
    {
        private int _index;
        private string _label;
        private NodeParams _params = new NodeParams();
        private List<Edge> _edges = new List<Edge>();

        public Node(NodeParams nodeParams = null, string label = "", int index = 0)
        {
            _params = nodeParams ?? new NodeParams();
            _index = index;
            _label = label;
        }

        [JsonProperty("name")]
        public string Label
        {
            get { return _label; }
            set { _label = value; }
        }

        [JsonProperty("params")]
        public NodeParams Params
        {
            get { return _params; }
            set { _params = value; }
        }

        public List<Edge> Edges
        {
            get { return _edges; }
            set { _edges = value; }
        }

        public override string ToString() 
        {
            var result = string.Format("{0}", _label, _key);

            if (_edges.Count == 0)
                return result + ";";
                
            result += ": ->";

            foreach (var edge in _edges)
            {
                var node = edge.EndNode;
                result += string.Format(" {0}", node._label, node._key);
                result += ",";
            }
            result = result.Remove(result.Length - 1);
            result += ";";

            return result;
        }
    }
}
