using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using GraphEditorWPF.Models.NodeModels;
using GraphEditorWPF.Models.EdgeModels;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace GraphEditorWPF.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Graph
    {
        private List<Node> _nodes = new List<Node>();
        private List<Edge> _edges = new List<Edge>();

        [JsonProperty("nodes")]
        public List<Node> Nodes
        {
            get { return _nodes; }
            set { _nodes = value; }
        }

        [JsonProperty("edges")]
        public List<Edge> Edges
        {
            get { return _edges; }
            set { _edges = value; }
        }

        public string ToJson(Formatting format = Formatting.None)
        {
            return JsonConvert.SerializeObject(this, format);
        }

        public void FromJson(string value)
        {
            var graph = JsonConvert.DeserializeObject<Graph>(value);

            Nodes = graph.Nodes;
            Edges = graph.Edges;
        }
    }
}
