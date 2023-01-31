using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using GraphEditorWPF.Models.NodeModels;
using GraphEditorWPF.Models.EdgeModels;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Collections;
using System.Collections.ObjectModel;
using System.Numerics;
using GraphEditorWPF.Utils;
using GraphEditorWPF.Types;
using System.Reflection;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;

namespace GraphEditorWPF.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Graph
    {
        private ObservableCollection<Node> _nodes = new ObservableCollection<Node>();
        private ObservableCollection<Edge> _edges = new ObservableCollection<Edge>();

        [JsonProperty("nodes")]
        public ObservableCollection<Node> Nodes
        {
            get { return _nodes; }
            set { _nodes = value; }
        }

        [JsonProperty("edges")]
        public ObservableCollection<Edge> Edges
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

        private double[,] GetGraphMatrix()
        {
            double[,] graphMatrix = new double[Nodes.Count, Nodes.Count];

            foreach (var edge in Edges)
            {
                var i = Nodes.IndexOf(edge.StartNode);
                var j = Nodes.IndexOf(edge.EndNode);

                if (i >= 0 && j >= 0)
                {
                    graphMatrix[i, j] = edge.Value;
                    graphMatrix[j, i] = edge.Value;
                }
            }

            return graphMatrix;
        }

        private void DFSRecursion(ref string output, Node node, Dictionary<string, bool> visited)
        {
            visited[node.Key] = true;

            output += node.Label;
            output += ", ";

            Node[] siblings = new Node[node.Edges.Count];
            for (var i = 0; i < node.Edges.Count; ++i)
            {
                siblings[i] = node.Edges[i].EndNode;
            }

            for (var i = 0; i < siblings.Length; ++i)
            {
                if (siblings[i] == null) continue;
                bool val;
                if (!visited.TryGetValue(siblings[i].Key, out val))
                    DFSRecursion(ref output, siblings[i], visited);
            }
        }

        public string DFS(Node start)
        {
            var visited = new Dictionary<string, bool>();
            var output = "";

            DFSRecursion(ref output, start, visited);

            return output.Substring(0, output.Length - 2);
        }

        private double heuristic(Node from, Node to)
        {
            return (Math.Sqrt(Math.Pow(from.Params.X - to.Params.X, 2) + Math.Pow(from.Params.Y - to.Params.Y, 2)));
        }

        private int findMin(Dictionary<int, double> dict)
        {
            var min = new KeyValuePair<int, double>(-1, double.MaxValue);
            foreach (KeyValuePair<int, double> entry in dict)
            {
                if (min.Value > entry.Value)
                {
                    min = entry;
                }
            }
            return min.Key;
        }

        public string Astar(Node start, Node end)
        {
            //var graph = GetGraphMatrix();

            Dictionary<int, double> open = new Dictionary<int, double>();
            Dictionary<int, int> closed = new Dictionary<int, int>();
            Dictionary<int, double> score = new Dictionary<int, double>();

            var startIndex = Nodes.IndexOf(start);
            open[startIndex] = heuristic(start, end);
            score[startIndex] = 0;

            var endIndex = Nodes.IndexOf(end);

            while (open.Count > 0)
            {
                var current = findMin(open);

                if (current == endIndex)
                {
                    Stack<int> path = new Stack<int>();
                    path.Push(current);
                    while (closed.ContainsKey(current))
                    {
                        current = closed[current];
                        path.Push(current);
                    }
                    var result = "";
                    while (path.Count > 0)
                    {
                        result += Nodes[path.Pop()].Label;
                        result += ", ";
                    }
                    return result.Substring(0, result.Length - 2);
                }
                open.Remove(current);

                foreach (var edge in Nodes[current].Edges)
                {
                    var index = Nodes.IndexOf(edge.EndNode);
                    if (index == current) continue;

                    var tentativeScore = edge.Weight + score[current];
                    var value = score.TryGetValue(index, out var val) ? val : double.MaxValue;

                    if (tentativeScore < value)
                    {
                        closed[index] = current;
                        score[index] = tentativeScore;
                        if (!open.ContainsKey(index))
                        {
                            open[index] = tentativeScore + heuristic(edge.EndNode, end);
                        }
                    }

                }
            }
            return "failure";

            //PriorityQueue<Tuple<int, double>> discoveredNodes = new PriorityQueue<Tuple<int, double>>();
            //Dictionary<int, double> shortestPathNodes = new Dictionary<int, double>();
            //List<double> shortestPathCosts = new List<double>();
            //Dictionary<int, double> chipiestPaths = new Dictionary<int, double>();

            //for (var i = 0; i < Nodes.Count; ++i)
            //{
            //    shortestPathCosts.Add(double.MaxValue);
            //}

            //var startIndex = Nodes.IndexOf(start);
            //discoveredNodes.Enqueue(startIndex);
            //distances.Add(double.MaxValue);

            //var endIndex = Nodes.IndexOf(end);
            //distances[endIndex] = 0;

            //while (boundaryNodes.Count > 0)
            //{
            //    int u = -1;
            //    double min = double.MaxValue;
            //    for (var i = 0; i < boundaryNodes.Count; ++i)
            //    {
            //        var node = Nodes[i];
            //        var potential = Math.Sqrt(Math.Pow(node.Params.X - end.Params.X, 2) + Math.Pow(node.Params.Y - end.Params.Y, 2));
            //        var value = distances[i] + potential;
            //        if (min > value)
            //        {
            //            min = value;
            //            u = i;
            //        }
            //    }

            //    if (u == endIndex)
            //    {
            //        return boundaryNodes.ToString();
            //    }
            //    boundaryNodes.RemoveAt(u);

            //    for (var i = 0; i < Nodes.Count; ++i)
            //    {
            //        double weight = graph[u, i];

            //        if (distances[i] > distances[u] + weight)
            //        {
            //            distances[i] = distances[u] + weight;
            //            boundaryNodes.Add(i);
            //        }
            //    }
            //}

            //return boundaryNodes.ToString();
        }
    }
}
